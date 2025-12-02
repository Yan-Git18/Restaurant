using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    public class Rest_DetallePedido
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, 999, ErrorMessage = "La cantidad debe estar entre 1 y 999")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "El subtotal es obligatorio")]
        [Range(0.01, 99999.99, ErrorMessage = "El subtotal debe ser mayor a 0")]
        [Column(TypeName = "decimal(10,2)")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal Subtotal { get; set; }

        [StringLength(200, ErrorMessage = "Las observaciones no pueden exceder 200 caracteres")]
        public string? Observaciones { get; set; }

        [Required(ErrorMessage = "El pedido es obligatorio")]
        [Display(Name = "Pedido")]
        public int PedidoId { get; set; }

        [Required(ErrorMessage = "El producto es obligatorio")]
        [Display(Name = "Producto")]
        public int ProductoId { get; set; }

        [ForeignKey("PedidoId")]
        public virtual Rest_Pedido Pedido { get; set; }

        [ForeignKey("ProductoId")]
        public virtual Rest_Producto Producto { get; set; }
    }
}
