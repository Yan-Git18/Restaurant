using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    public class Rest_Pago
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El método de pago es obligatorio")]
        [StringLength(30, ErrorMessage = "El método no puede exceder 30 caracteres")]
        [Display(Name = "Método")]
        public string Metodo { get; set; } 

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, 99999.99, ErrorMessage = "El monto debe ser mayor a 0")]
        [Column(TypeName = "decimal(10,2)")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal Monto { get; set; }

        [Display(Name = "Fecha")]
        public DateTime FechaPago { get; set; } 

        [StringLength(100, ErrorMessage = "La referencia no puede exceder 100 caracteres")]
        public string? Referencia { get; set; } 

        [StringLength(200, ErrorMessage = "Las observaciones no pueden exceder 200 caracteres")]
        public string? Observaciones { get; set; }

        [Required(ErrorMessage = "La venta es obligatoria")]
        [Display(Name = "Venta")]
        public int VentaId { get; set; }

        [ForeignKey("VentaId")]
        public virtual Rest_Venta Venta { get; set; }
    }
}
