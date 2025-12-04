using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRUEBATECNICA.STEFFANINIGROUP.INVENTARIO.Models
{
    /// <summary>
    /// Tabla de Inventario de Productos (API-PRODUCTOS)
    /// </summary>
    [Table("Productos")]
    public class Producto
    {
        [Key]
        [Column("idpro")]
        public int IdPro { get; set; }

        [Required]
        [StringLength(200)]
        [Column("producto")]
        public string ProductoNombre { get; set; } = string.Empty;

        [Required]
        [Column("precio", TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        // Navegación
        public virtual ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
    }
}
