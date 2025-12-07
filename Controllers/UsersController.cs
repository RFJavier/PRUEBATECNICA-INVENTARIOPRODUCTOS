using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using restapi.inventarios.Data;
using restapi.inventarios.Entities;
using restapi.inventarios.Models;

namespace restapi.inventarios.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public UsersController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var users = await _db.Users.Select(u => new
            {
                u.Id,
                u.Username,
                u.IsActive,
                Roles = u.UserRoles.Select(ur => ur.Role!.Name)
            }).ToListAsync();
            return Ok(ApiResponse<object>.Ok(users, $"Se encontraron {users.Count} usuarios"));
        }

        [HttpPost("{id:int}/roles")]
        public async Task<IActionResult> AssignRole(int id, [FromBody] string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return BadRequest(ApiResponse.Fail("Nombre del rol es requerido"));

            var user = await _db.Users.Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return NotFound(ApiResponse.Fail($"Usuario con ID {id} no encontrado"));

            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null)
                return BadRequest(ApiResponse.Fail($"El rol '{roleName}' no existe"));

            if (user.UserRoles.Any(ur => ur.RoleId == role.Id))
                return Conflict(ApiResponse.Fail($"El usuario ya tiene el rol '{roleName}'"));

            _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
            await _db.SaveChangesAsync();
            return Ok(ApiResponse.Ok($"Rol '{roleName}' asignado exitosamente al usuario '{user.Username}'"));
        }

        [HttpDelete("{id:int}/roles/{roleName}")]
        public async Task<IActionResult> RemoveRole(int id, string roleName)
        {
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null)
                return NotFound(ApiResponse.Fail($"El rol '{roleName}' no existe"));

            var ur = await _db.UserRoles.FirstOrDefaultAsync(x => x.UserId == id && x.RoleId == role.Id);
            if (ur == null)
                return NotFound(ApiResponse.Fail($"El usuario no tiene el rol '{roleName}'"));

            _db.UserRoles.Remove(ur);
            await _db.SaveChangesAsync();
            return Ok(ApiResponse.Ok($"Rol '{roleName}' removido exitosamente"));
        }
    }
}
