using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRUEBATECNICA.STEFFANINIGROUP.INVENTARIO.Models
{
    /// <summary>
    /// Tabla de Detalle de Ventas (API-VENTAS)
    /// </summary>
    [Table("DetalleVentas")]
    public class DetalleVenta
    {
        [Key]
        [Column("idde")]
        public int IdDe { get; set; }

        [Required]
        [Column("fecha")]
        public DateTime Fecha { get; set; }

        [Required]
        [Column("idventa")]
        public int IdVenta { get; set; }

        [Required]
        [Column("idpro")]
        public int IdPro { get; set; }

        [Required]
        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Required]
        [Column("precio", TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        [Required]
        [Column("iva", TypeName = "decimal(18,2)")]
        public decimal Iva { get; set; }

        [Required]
        [Column("total", TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        // Navegación
        [ForeignKey(nameof(IdVenta))]
        public virtual EncabezadoVenta EncabezadoVenta { get; set; } = null!;

        [ForeignKey(nameof(IdPro))]
        public virtual Producto Producto { get; set; } = null!;
    }
}
