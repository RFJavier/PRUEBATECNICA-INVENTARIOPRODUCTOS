namespace WEBAPP.Models;

public class Venta
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string Vendedor { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public List<DetalleVenta> Detalles { get; set; } = new();
}

public class DetalleVenta
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public int Idpro { get; set; }
    public int Cantidad { get; set; }
    public decimal Precio { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
}

public class VentaRequest
{
    public DateTime Fecha { get; set; }
    public string Vendedor { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public List<DetalleVentaRequest> Detalles { get; set; } = new();
}

public class DetalleVentaRequest
{
    public DateTime Fecha { get; set; }
    public int Idpro { get; set; }
    public int Cantidad { get; set; }
    public decimal Precio { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
}
