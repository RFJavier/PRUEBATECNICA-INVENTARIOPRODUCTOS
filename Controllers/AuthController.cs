using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using restapi.inventarios.Data;
using restapi.inventarios.Security;
using restapi.inventarios.Entities;
using Microsoft.EntityFrameworkCore;

namespace restapi.inventarios.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _db;

        public AuthController(IConfiguration config, AppDbContext db)
        {
            _config = config;
            _db = db;
        }

        public record LoginRequest(string Username, string Password);
        public record RegisterRequest(string Username, string Password);

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _db.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                                       .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);
            if (user is null)
                return Unauthorized();

            var valid = PasswordHasher.Verify(request.Password, user.PasswordSalt, user.PasswordHash);
            if (!valid)
                return Unauthorized();

            var roles = user.UserRoles.Select(ur => ur.Role!.Name).ToArray();
            var (token, jti, expiresAt) = CreateToken(user.Username, roles);

            // Registrar sesión
            var session = new Session
            {
                UserId = user.Id,
                JwtId = jti,
                UserAgent = Request.Headers.UserAgent.ToString(),
                IP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt
            };
            _db.Sessions.Add(session);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                access_token = token,
                token_type = "Bearer",
                expires_in = (int)(expiresAt - DateTime.UtcNow).TotalSeconds,
                username = user.Username,
                roles
            });
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Usuario y contraseña requeridos");

            var exists = await _db.Users.AnyAsync(u => u.Username == request.Username);
            if (exists) return Conflict("Usuario ya existe");

            var salt = PasswordHasher.GenerateSalt();
            var hash = PasswordHasher.HashPassword(request.Password, salt);

            var user = new User
            {
                Username = request.Username,
                PasswordSalt = salt,
                PasswordHash = hash,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // asignar rol por defecto "User" si existe
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (role != null)
            {
                _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
                await _db.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(Login), new { username = user.Username }, new { user.Id, user.Username });
        }

        private (string token, string jti, DateTime expiresAt) CreateToken(string username, IEnumerable<string> roles)
        {
            var jwtKey = _config["Jwt:Key"] ?? "ClaveSuperSecreta_development_cambia_esto";
            var jwtIssuer = _config["Jwt:Issuer"] ?? "restapi.inventarios";
            var jwtAudience = _config["Jwt:Audience"] ?? "restapi.inventarios.clients";

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var jti = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Jti, jti),
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var expires = DateTime.UtcNow.AddHours(1);
            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expires,
                signingCredentials: credentials
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), jti, expires);
        }
    }
}
