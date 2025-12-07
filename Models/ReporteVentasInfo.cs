namespace WEBAPP.Models;

public class ReporteVentasInfo
{
    public int TotalVentas { get; set; }
    public decimal TotalMonto { get; set; }
    public decimal PromedioVenta { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
}
