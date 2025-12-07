using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using restapi.inventarios.Data.Repositories;
using restapi.inventarios.Entities;

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
            return Ok(list);
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var (enc, dets) = await _repo.GetByIdAsync(id);
            if (enc == null) return NotFound();
            return Ok(new { encabezado = enc, detalles = dets });
        }

        public record DetalleReq(DateTime fecha, int idpro, int cantidad, decimal precio, decimal iva, decimal total);
        public record CreateReq(DateTime fecha, string vendedor, decimal total, List<DetalleReq> detalles);

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateReq req)
        {
            var enc = new EncabezadoVenta { Fecha = req.fecha, Vendedor = req.vendedor, Total = req.total };
            var dets = req.detalles.Select(d => new DetalleVenta
            {
                Fecha = d.fecha, IdProducto = d.idpro, Cantidad = d.cantidad, Precio = d.precio, Iva = d.iva, Total = d.total
            }).ToList();
            var id = await _repo.CreateAsync(enc, dets);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        public record UpdateReq(DateTime fecha, string vendedor, decimal total, List<DetalleReq> detalles);

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateReq req)
        {
            var enc = new EncabezadoVenta { Id = id, Fecha = req.fecha, Vendedor = req.vendedor, Total = req.total };
            var dets = req.detalles.Select(d => new DetalleVenta
            {
                Fecha = d.fecha, IdProducto = d.idpro, Cantidad = d.cantidad, Precio = d.precio, Iva = d.iva, Total = d.total
            }).ToList();
            var ok = await _repo.UpdateAsync(enc, dets);
            return ok ? Ok() : NotFound();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _repo.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }
    }
}
