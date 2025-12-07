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
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public RolesController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var roles = await _db.Roles.OrderBy(r => r.Name).ToListAsync();
            return Ok(ApiResponse<object>.Ok(roles, $"Se encontraron {roles.Count} roles"));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Role role)
        {
            if (string.IsNullOrWhiteSpace(role.Name))
                return BadRequest(ApiResponse.Fail("Nombre del rol es requerido"));

            var exists = await _db.Roles.AnyAsync(r => r.Name == role.Name);
            if (exists)
                return Conflict(ApiResponse.Fail($"El rol '{role.Name}' ya existe"));

            _db.Roles.Add(role);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = role.Id },
                ApiResponse<object>.Created(role, "Rol creado exitosamente"));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var role = await _db.Roles.FindAsync(id);
            if (role == null)
                return NotFound(ApiResponse.Fail($"Rol con ID {id} no encontrado"));

            _db.Roles.Remove(role);
            await _db.SaveChangesAsync();
            return Ok(ApiResponse.Ok("Rol eliminado exitosamente"));
        }
    }
}
