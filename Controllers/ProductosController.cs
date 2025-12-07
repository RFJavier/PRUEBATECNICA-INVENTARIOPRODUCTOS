using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using restapi.inventarios.Data.Repositories;
using restapi.inventarios.Models;

namespace restapi.inventarios.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly ProductoRepository _repo;
        public ProductosController(ProductoRepository repo) { _repo = repo; }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var list = await _repo.GetAllAsync();
            return Ok(ApiResponse<object>.Ok(list, $"Se encontraron {list.Count} productos"));
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item is null)
                return NotFound(ApiResponse.Fail($"Producto con ID {id} no encontrado"));
            return Ok(ApiResponse<object>.Ok(item, "Producto encontrado"));
        }

        /// <summary>
        /// Buscar productos por código/nombre (búsqueda parcial)
        /// </summary>
        [HttpGet("buscar")]
        [Authorize]
        public async Task<IActionResult> SearchByCodigo([FromQuery] string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return BadRequest(ApiResponse.Fail("El parámetro 'codigo' es requerido"));

            var items = await _repo.SearchByCodigoAsync(codigo);
            return Ok(ApiResponse<object>.Ok(items, $"Se encontraron {items.Count} productos"));
        }

        /// <summary>
        /// Buscar producto por código/nombre exacto
        /// </summary>
        [HttpGet("buscar-exacto")]
        [Authorize]
        public async Task<IActionResult> GetByCodigoExacto([FromQuery] string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return BadRequest(ApiResponse.Fail("El parámetro 'codigo' es requerido"));

            var item = await _repo.GetByCodigoExactoAsync(codigo);
            if (item is null)
                return NotFound(ApiResponse.Fail($"No se encontró producto con código '{codigo}'"));

            return Ok(ApiResponse<object>.Ok(item, "Producto encontrado"));
        }

        public record CreateRequest(string codigo, string producto, decimal precio);

        [HttpPost]
        [Authorize(Roles = "Bodeguero,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.codigo))
                return BadRequest(ApiResponse.Fail("El código es requerido"));
            if (string.IsNullOrWhiteSpace(req.producto))
                return BadRequest(ApiResponse.Fail("El nombre del producto es requerido"));

            var id = await _repo.CreateAsync(req.codigo, req.producto, req.precio);
            return CreatedAtAction(nameof(GetById), new { id }, 
                ApiResponse<object>.Created(new { id, req.codigo, req.producto, req.precio }, "Producto creado exitosamente"));
        }

        public record UpdateRequest(string codigo, string producto, decimal precio);

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Bodeguero,Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.codigo))
                return BadRequest(ApiResponse.Fail("El código es requerido"));
            if (string.IsNullOrWhiteSpace(req.producto))
                return BadRequest(ApiResponse.Fail("El nombre del producto es requerido"));

            var ok = await _repo.UpdateAsync(id, req.codigo, req.producto, req.precio);
            if (!ok)
                return NotFound(ApiResponse.Fail($"Producto con ID {id} no encontrado"));
            return Ok(ApiResponse.Ok("Producto actualizado exitosamente"));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Bodeguero,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _repo.DeleteAsync(id);
            if (!ok)
                return NotFound(ApiResponse.Fail($"Producto con ID {id} no encontrado"));
            return Ok(ApiResponse.Ok("Producto eliminado exitosamente"));
        }
    }
}
