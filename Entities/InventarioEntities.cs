namespace restapi.inventarios.Entities
{
    public class Producto
    {
        public int Id { get; set; } // idpro
        public string Nombre { get; set; } = string.Empty; // producto
        public decimal Precio { get; set; } // precio
    }

    public class EncabezadoVenta
    {
        public int Id { get; set; } // idventa
        public DateTime Fecha { get; set; }
        public string Vendedor { get; set; } = string.Empty;
        public decimal Total { get; set; }

        public List<DetalleVenta> Detalles { get; set; } = new();
    }

    public class DetalleVenta
    {
        public int Id { get; set; } // idde
        public DateTime Fecha { get; set; }
        public int IdVenta { get; set; }
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }

        public EncabezadoVenta? Encabezado { get; set; }
        public Producto? Producto { get; set; }
    }

    public class LogProducto
    {
        public int Id { get; set; } // idlog
        public string? Operacion { get; set; }
        public int? IdProducto { get; set; }
        public string? ProductoOld { get; set; }
        public decimal? PrecioOld { get; set; }
        public string? ProductoNew { get; set; }
        public decimal? PrecioNew { get; set; }
        public DateTime Fecha { get; set; }
    }
}
