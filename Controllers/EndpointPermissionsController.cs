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
    public class EndpointPermissionsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public EndpointPermissionsController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var perms = await _db.EndpointPermissions.ToListAsync();
            return Ok(ApiResponse<object>.Ok(perms, $"Se encontraron {perms.Count} permisos de endpoint"));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EndpointPermission model)
        {
            if (string.IsNullOrWhiteSpace(model.Route) || string.IsNullOrWhiteSpace(model.HttpMethod))
                return BadRequest(ApiResponse.Fail("Route y HttpMethod son requeridos"));

            model.HttpMethod = model.HttpMethod.ToUpperInvariant();
            _db.EndpointPermissions.Add(model);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = model.Id },
                ApiResponse<object>.Created(model, "Permiso de endpoint creado exitosamente"));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] EndpointPermission model)
        {
            var exist = await _db.EndpointPermissions.FindAsync(id);
            if (exist == null)
                return NotFound(ApiResponse.Fail($"Permiso con ID {id} no encontrado"));

            exist.Route = model.Route;
            exist.HttpMethod = model.HttpMethod.ToUpperInvariant();
            exist.AllowedRolesCsv = model.AllowedRolesCsv;
            exist.IsEnabled = model.IsEnabled;
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(exist, "Permiso de endpoint actualizado exitosamente"));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var exist = await _db.EndpointPermissions.FindAsync(id);
            if (exist == null)
                return NotFound(ApiResponse.Fail($"Permiso con ID {id} no encontrado"));

            _db.EndpointPermissions.Remove(exist);
            await _db.SaveChangesAsync();
            return Ok(ApiResponse.Ok("Permiso de endpoint eliminado exitosamente"));
        }
    }
}
