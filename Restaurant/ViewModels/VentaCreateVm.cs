using System.ComponentModel.DataAnnotations;

namespace Restaurant.ViewModels
{
    public class VentaCreateVm
    {
        [Required]
        public int PedidoId { get; set; }

        [Range(0, 99999.99)]
        public decimal Descuento { get; set; } = 0;

        [Range(0, 1)]
        public decimal Impuesto { get; set; } = 0.18m;
    }
}
