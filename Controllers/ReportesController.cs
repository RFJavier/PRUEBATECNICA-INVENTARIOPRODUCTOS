using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using restapi.inventarios.Data.Repositories;
using restapi.inventarios.Models;
using restapi.inventarios.Services;

namespace restapi.inventarios.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Operador,Admin")]
    public class ReportesController : ControllerBase
    {
        private readonly VentasRepository _ventasRepo;
        private readonly ReportesService _reportesService;

        public ReportesController(VentasRepository ventasRepo, ReportesService reportesService)
        {
            _ventasRepo = ventasRepo;
            _reportesService = reportesService;
        }

        /// <summary>
        /// Genera reporte de ventas en formato Excel
        /// </summary>
        /// <param name="fechaInicio">Fecha inicial del período (opcional)</param>
        /// <param name="fechaFin">Fecha final del período (opcional)</param>
        [HttpGet("ventas/excel")]
        public async Task<IActionResult> GenerarExcelVentas([FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
        {
            try
            {
                var ventas = await ObtenerVentasParaReporte(fechaInicio, fechaFin);

                if (ventas.Count == 0)
                    return NotFound(ApiResponse.Fail("No se encontraron ventas en el período especificado"));

                var excelBytes = _reportesService.GenerarExcelVentas(ventas, fechaInicio, fechaFin);

                var fileName = $"Reporte_Ventas_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Fail($"Error al generar el reporte Excel: {ex.Message}"));
            }
        }

        /// <summary>
        /// Genera reporte de ventas en formato PDF
        /// </summary>
        /// <param name="fechaInicio">Fecha inicial del período (opcional)</param>
        /// <param name="fechaFin">Fecha final del período (opcional)</param>
        [HttpGet("ventas/pdf")]
        public async Task<IActionResult> GenerarPdfVentas([FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
        {
            try
            {
                var ventas = await ObtenerVentasParaReporte(fechaInicio, fechaFin);

                if (ventas.Count == 0)
                    return NotFound(ApiResponse.Fail("No se encontraron ventas en el período especificado"));

                var pdfBytes = _reportesService.GenerarPdfVentas(ventas, fechaInicio, fechaFin);

                var fileName = $"Reporte_Ventas_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Fail($"Error al generar el reporte PDF: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene información del reporte disponible (sin descargar)
        /// </summary>
        [HttpGet("ventas/info")]
        public async Task<IActionResult> ObtenerInfoReporte([FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
        {
            try
            {
                var ventas = await ObtenerVentasParaReporte(fechaInicio, fechaFin);

                var info = new
                {
                    totalVentas = ventas.Count,
                    totalMonto = ventas.Sum(v => v.Total),
                    promedioVenta = ventas.Count > 0 ? ventas.Average(v => v.Total) : 0,
                    ventaMayor = ventas.Count > 0 ? ventas.Max(v => v.Total) : 0,
                    ventaMenor = ventas.Count > 0 ? ventas.Min(v => v.Total) : 0,
                    fechaInicio = fechaInicio,
                    fechaFin = fechaFin,
                    formatosDisponibles = new[] { "pdf", "excel" },
                    urlPdf = $"/api/reportes/ventas/pdf{BuildQueryString(fechaInicio, fechaFin)}",
                    urlExcel = $"/api/reportes/ventas/excel{BuildQueryString(fechaInicio, fechaFin)}"
                };

                return Ok(ApiResponse<object>.Ok(info, $"Reporte disponible con {ventas.Count} ventas"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Fail($"Error al obtener información del reporte: {ex.Message}"));
            }
        }

        private async Task<List<VentaReporteItem>> ObtenerVentasParaReporte(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var ventasConDetalles = await _ventasRepo.GetAllWithDetallesForReportAsync();

            // Filtrar por fechas si se especifican
            if (fechaInicio.HasValue)
                ventasConDetalles = ventasConDetalles.Where(v => v.Fecha >= fechaInicio.Value).ToList();

            if (fechaFin.HasValue)
                ventasConDetalles = ventasConDetalles.Where(v => v.Fecha <= fechaFin.Value.AddDays(1).AddSeconds(-1)).ToList();

            return ventasConDetalles;
        }

        private static string BuildQueryString(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var parts = new List<string>();
            if (fechaInicio.HasValue)
                parts.Add($"fechaInicio={fechaInicio:yyyy-MM-dd}");
            if (fechaFin.HasValue)
                parts.Add($"fechaFin={fechaFin:yyyy-MM-dd}");

            return parts.Count > 0 ? "?" + string.Join("&", parts) : "";
        }
    }
}
