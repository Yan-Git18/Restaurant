using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    public class Rest_Reserva
    {
        [Key]
        public int ReservaId { get; set; }

        [Required(ErrorMessage = "La fecha y hora de reserva es obligatoria")]
        [Display(Name = "Fecha y Hora")]
        public DateTime FechaHora { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        [StringLength(20, ErrorMessage = "El estado no puede exceder 20 caracteres")]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Confirmada, Cancelada, Completada

        [Range(1, 20, ErrorMessage = "El número de personas debe estar entre 1 y 20")]
        [Display(Name = "Número de Personas")]
        public int NumeroPersonas { get; set; }

        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        public string? Observaciones { get; set; }

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } //= DateTime.Now;

        [Required(ErrorMessage = "El cliente es obligatorio")]
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "La mesa es obligatoria")]
        public int MesaId { get; set; }

        // Navegación
        [ForeignKey("ClienteId")]
        public virtual Rest_Persona Cliente { get; set; }

        [ForeignKey("MesaId")]
        public virtual Rest_Mesa Mesa { get; set; }
    }
}
