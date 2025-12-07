using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using restapi.inventarios.Data.Repositories;

namespace restapi.inventarios.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly ProductoRepository _repo;
        public ProductosController(ProductoRepository repo) { _repo = repo; }

        [HttpGet]
        [Authorize] // requiere token
        public async Task<IActionResult> GetAll() => Ok(await _repo.GetAllAsync());

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            return item is null ? NotFound() : Ok(item);
        }

        public record CreateRequest(string producto, decimal precio);

        [HttpPost]
        [Authorize(Roles = "Admin")] // solo admin crea
        public async Task<IActionResult> Create([FromBody] CreateRequest req)
        {
            var id = await _repo.CreateAsync(req.producto, req.precio);
            return CreatedAtAction(nameof(GetById), new { id }, new { id, req.producto, req.precio });
        }

        public record UpdateRequest(string producto, decimal precio);

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")] // solo admin edita
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRequest req)
        {
            var ok = await _repo.UpdateAsync(id, req.producto, req.precio);
            return ok ? Ok() : NotFound();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")] // solo admin borra
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _repo.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }
    }
}
