namespace WEBAPP.Models;

public class Producto
{
    public int Id { get; set; }
    public string Producto1 { get; set; } = string.Empty;
    public decimal Precio { get; set; }
}

public class ProductoRequest
{
    public string Producto { get; set; } = string.Empty;
    public decimal Precio { get; set; }
}
