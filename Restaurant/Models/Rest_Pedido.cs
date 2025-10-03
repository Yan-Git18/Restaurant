using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    public class Rest_Pedido
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha del pedido es obligatoria")]
        [Display(Name = "Fecha")]
        public DateTime Fecha { get; set; } 

        [Required(ErrorMessage = "El estado es obligatorio")]
        [StringLength(20, ErrorMessage = "El estado no puede exceder 20 caracteres")]
        public string Estado { get; set; } = "Pendiente";

        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        public string? Observaciones { get; set; }

        [Display(Name = "Tipo de Pedido")]
        [StringLength(20, ErrorMessage = "El tipo de pedido no puede exceder 20 caracteres")]
        public string TipoPedido { get; set; } = "Mesa";

        [Required(ErrorMessage = "El cliente es obligatorio")]
        [Display(Name = "Cliente")]
        public int ClienteId { get; set; }

        [Display(Name = "Mesa")]
        public int? MesaId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 9999999, ErrorMessage = "El total debe ser mayor o igual a 0")]
        [Display(Name = "Total del Pedido")]
        public decimal Total { get; set; } = 0;

        // Navegación
        [ForeignKey("ClienteId")]
        public virtual Rest_Persona Cliente { get; set; }

        [ForeignKey("MesaId")]
        public virtual Rest_Mesa? Mesa { get; set; }

        public virtual ICollection<Rest_DetallePedido> DetallesPedido { get; set; } = new List<Rest_DetallePedido>();

        public virtual Rest_Venta? Venta { get; set; }
    }
}
