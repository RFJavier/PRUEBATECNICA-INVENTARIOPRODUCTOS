using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using restapi.inventarios.Data.Repositories;
using restapi.inventarios.Entities;
using restapi.inventarios.Models;

namespace restapi.inventarios.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VentasController : ControllerBase
    {
        private readonly VentasRepository _repo;
        public VentasController(VentasRepository repo) { _repo = repo; }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var list = await _repo.GetAllAsync();
            return Ok(ApiResponse<object>.Ok(list, $"Se encontraron {list.Count} ventas"));
        }

        [HttpGet("con-detalles")]
        [Authorize(Roles = "Operador,Admin,Supervisor")]
        public async Task<IActionResult> GetAllWithDetalles()
        {
            var rows = await _repo.GetAllWithDetallesAsync();
            return Ok(ApiResponse<object>.Ok(rows, $"Se encontraron {rows.Count} ventas con detalles"));
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var (enc, dets) = await _repo.GetByIdAsync(id);
            if (enc == null)
                return NotFound(ApiResponse.Fail($"Venta con ID {id} no encontrada"));
            return Ok(ApiResponse<object>.Ok(new { encabezado = enc, detalles = dets }, "Venta encontrada"));
        }

        public record DetalleReq(DateTime fecha, int idpro, int cantidad, decimal precio, decimal iva, decimal total);
        public record CreateReq(DateTime fecha, string vendedor, decimal total, List<DetalleReq> detalles);

        [HttpPost]
        [Authorize(Roles = "Operador,Admin,Supervisor")]
        public async Task<IActionResult> Create([FromBody] CreateReq req)
        {
            if (string.IsNullOrWhiteSpace(req.vendedor))
                return BadRequest(ApiResponse.Fail("El vendedor es requerido"));
            if (req.detalles == null || req.detalles.Count == 0)
                return BadRequest(ApiResponse.Fail("Debe incluir al menos un detalle de venta"));

            var enc = new EncabezadoVenta { Fecha = req.fecha, Vendedor = req.vendedor, Total = req.total };
            var dets = req.detalles.Select(d => new DetalleVenta
            {
                Fecha = d.fecha, IdProducto = d.idpro, Cantidad = d.cantidad, Precio = d.precio, Iva = d.iva, Total = d.total
            }).ToList();
            var id = await _repo.CreateAsync(enc, dets);
            return CreatedAtAction(nameof(GetById), new { id },
                ApiResponse<object>.Created(new { id }, "Venta creada exitosamente"));
        }

        public record UpdateReq(DateTime fecha, string vendedor, decimal total, List<DetalleReq> detalles);

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Operador,Admin,Supervisor")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateReq req)
        {
            if (string.IsNullOrWhiteSpace(req.vendedor))
                return BadRequest(ApiResponse.Fail("El vendedor es requerido"));

            var enc = new EncabezadoVenta { Id = id, Fecha = req.fecha, Vendedor = req.vendedor, Total = req.total };
            var dets = req.detalles.Select(d => new DetalleVenta
            {
                Fecha = d.fecha, IdProducto = d.idpro, Cantidad = d.cantidad, Precio = d.precio, Iva = d.iva, Total = d.total
            }).ToList();
            var ok = await _repo.UpdateAsync(enc, dets);
            if (!ok)
                return NotFound(ApiResponse.Fail($"Venta con ID {id} no encontrada"));
            return Ok(ApiResponse.Ok("Venta actualizada exitosamente"));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Operador,Admin,Supervisor")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _repo.DeleteAsync(id);
            if (!deleted)
                return NotFound(ApiResponse.Fail($"Venta con ID {id} no encontrada"));
            return Ok(ApiResponse.Ok("Venta eliminada exitosamente"));
        }
    }
}
