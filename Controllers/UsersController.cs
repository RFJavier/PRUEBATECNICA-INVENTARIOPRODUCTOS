using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using restapi.inventarios.Data;
using restapi.inventarios.Entities;

namespace restapi.inventarios.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // solo admin gestiona usuarios
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public UsersController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> Get() => Ok(await _db.Users.Select(u => new
        {
            u.Id,
            u.Username,
            u.IsActive,
            Roles = u.UserRoles.Select(ur => ur.Role!.Name)
        }).ToListAsync());

        [HttpPost("{id:int}/roles")]
        public async Task<IActionResult> AssignRole(int id, [FromBody] string roleName)
        {
            var user = await _db.Users.Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound("Usuario no existe");
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null) return BadRequest("Rol no válido");
            if (user.UserRoles.Any(ur => ur.RoleId == role.Id)) return Conflict("Usuario ya tiene ese rol");
            _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id:int}/roles/{roleName}")]
        public async Task<IActionResult> RemoveRole(int id, string roleName)
        {
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null) return NotFound("Rol no existe");
            var ur = await _db.UserRoles.FirstOrDefaultAsync(x => x.UserId == id && x.RoleId == role.Id);
            if (ur == null) return NotFound();
            _db.UserRoles.Remove(ur);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
