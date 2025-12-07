using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using restapi.inventarios.Data;
using restapi.inventarios.Entities;

namespace restapi.inventarios.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // solo admin puede gestionar roles
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public RolesController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> Get() => Ok(await _db.Roles.OrderBy(r => r.Name).ToListAsync());

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Role role)
        {
            if (string.IsNullOrWhiteSpace(role.Name)) return BadRequest("Nombre requerido");
            var exists = await _db.Roles.AnyAsync(r => r.Name == role.Name);
            if (exists) return Conflict("Rol ya existe");
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = role.Id }, role);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var role = await _db.Roles.FindAsync(id);
            if (role == null) return NotFound();
            _db.Roles.Remove(role);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
