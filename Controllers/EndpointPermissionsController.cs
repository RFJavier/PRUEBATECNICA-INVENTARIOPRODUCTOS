using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using restapi.inventarios.Data;
using restapi.inventarios.Entities;

namespace restapi.inventarios.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // solo admin gestiona permisos de endpoints
    public class EndpointPermissionsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public EndpointPermissionsController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> Get() => Ok(await _db.EndpointPermissions.ToListAsync());

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EndpointPermission model)
        {
            if (string.IsNullOrWhiteSpace(model.Route) || string.IsNullOrWhiteSpace(model.HttpMethod))
                return BadRequest("Route y HttpMethod requeridos");
            model.HttpMethod = model.HttpMethod.ToUpperInvariant();
            _db.EndpointPermissions.Add(model);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] EndpointPermission model)
        {
            var exist = await _db.EndpointPermissions.FindAsync(id);
            if (exist == null) return NotFound();
            exist.Route = model.Route;
            exist.HttpMethod = model.HttpMethod.ToUpperInvariant();
            exist.AllowedRolesCsv = model.AllowedRolesCsv;
            exist.IsEnabled = model.IsEnabled;
            await _db.SaveChangesAsync();
            return Ok(exist);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var exist = await _db.EndpointPermissions.FindAsync(id);
            if (exist == null) return NotFound();
            _db.EndpointPermissions.Remove(exist);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
