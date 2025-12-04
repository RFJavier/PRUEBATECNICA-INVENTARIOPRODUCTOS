using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRUEBATECNICA.STEFFANINIGROUP.INVENTARIO.Models
{
    /// <summary>
    /// Tabla de Encabezado de Ventas (API-VENTAS)
    /// </summary>
    [Table("EncabezadoVentas")]
    public class EncabezadoVenta
    {
        [Key]
        [Column("idventa")]
        public int IdVenta { get; set; }

        [Required]
        [Column("fecha")]
        public DateTime Fecha { get; set; }

        [Required]
        [StringLength(150)]
        [Column("vendedor")]
        public string Vendedor { get; set; } = string.Empty;

        [Required]
        [Column("total", TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        // Navegación
        public virtual ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
    }
}
