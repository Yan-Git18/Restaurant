using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    public class Rest_Comprobante
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El tipo es obligatorio")]
        [StringLength(20, ErrorMessage = "El tipo no puede exceder 20 caracteres")]
        public string Tipo { get; set; } // Boleta, Factura, Ticket

        [Required(ErrorMessage = "El número es obligatorio")]
        [StringLength(20, ErrorMessage = "El número no puede exceder 20 caracteres")]
        public string Numero { get; set; }

        [StringLength(50, ErrorMessage = "El formato no puede exceder 50 caracteres")]
        public string Formato { get; set; } = "PDF"; // PDF, XML, etc.

        [Display(Name = "Fecha de Emisión")]
        public DateTime FechaEmision { get; set; } //= DateTime.Now;

        [Display(Name = "Enviado por Email")]
        public bool EnviadoEmail { get; set; } = false;

        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        public string? Observaciones { get; set; }

        [Required(ErrorMessage = "La venta es obligatoria")]
        [Display(Name = "Venta")]
        public int VentaId { get; set; }

        // Navegación
        [ForeignKey("VentaId")]
        public virtual Rest_Venta Venta { get; set; }
    }
}
