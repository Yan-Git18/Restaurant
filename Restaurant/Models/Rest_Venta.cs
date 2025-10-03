using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    public class Rest_Venta
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha de venta es obligatoria")]
        [Display(Name = "Fecha")]
        public DateTime Fecha { get; set; } 

        [Required(ErrorMessage = "El total es obligatorio")]
        [Range(0.01, 99999.99, ErrorMessage = "El total debe ser mayor a 0")]
        [Column(TypeName = "decimal(10,2)")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal Total { get; set; }

        [Range(0, 99999.99, ErrorMessage = "El descuento no puede ser negativo")]
        [Column(TypeName = "decimal(10,2)")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal Descuento { get; set; } = 0;

        [Range(0, 99.99, ErrorMessage = "El impuesto debe estar entre 0 y 99.99")]
        [Column(TypeName = "decimal(5,2)")]
        [DisplayFormat(DataFormatString = "{0:P2}", ApplyFormatInEditMode = false)]
        public decimal Impuesto { get; set; } = 0.18m; // 18% IGV por defecto

        [Required(ErrorMessage = "El estado es obligatorio")]
        [StringLength(20, ErrorMessage = "El estado no puede exceder 20 caracteres")]
        public string Estado { get; set; } = "Pendiente";

        [Required(ErrorMessage = "El pedido es obligatorio")]
        [Display(Name = "Pedido")]
        public int PedidoId { get; set; }

        // Navegación
        [ForeignKey("PedidoId")]
        public virtual Rest_Pedido Pedido { get; set; }

        public virtual ICollection<Rest_Pago> Pagos { get; set; } = new List<Rest_Pago>();
        public virtual Rest_Comprobante? Comprobante { get; set; }
    }
}
