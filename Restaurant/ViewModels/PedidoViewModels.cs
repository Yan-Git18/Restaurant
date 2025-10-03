using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Restaurant.ViewModels
{
    public class DetallePedidoVm
    {
        [Required]
        public int ProductoId { get; set; }

        [Required]
        [Range(1, 999, ErrorMessage = "La cantidad debe estar entre 1 y 999")]
        public int Cantidad { get; set; }

        [Required]
        [Range(0.01, 99999, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Precio { get; set; } 
    }

    public class PedidoCreateEditVm
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha del pedido es obligatoria")]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El estado es obligatorio")]
        [StringLength(20, ErrorMessage = "El estado no puede exceder 20 caracteres")]
        public string Estado { get; set; } = "Pendiente";

        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        public string? Observaciones { get; set; }

        [StringLength(20, ErrorMessage = "El tipo de pedido no puede exceder 20 caracteres")]
        public string TipoPedido { get; set; } = "Mesa";

        [Required(ErrorMessage = "El cliente es obligatorio")]
        public int ClienteId { get; set; }

        public int? MesaId { get; set; }

        // Lista de detalles enviada desde la vista
        public List<DetallePedidoVm> Detalles { get; set; } = new List<DetallePedidoVm>();

        [Display(Name = "Total del Pedido")]
        public decimal Total { get; set; } // se calcula en el controlador antes de mostrar la vista
    }
}
