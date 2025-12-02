using System;
using System.ComponentModel.DataAnnotations;

namespace Restaurant.ViewModels
{
    public class ReservaFormViewModel
    {
        public int? ClienteId { get; set; } 

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Phone(ErrorMessage = "El teléfono no es válido")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo no es válido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "La hora es obligatoria")]
        public string Hora { get; set; }

        [Required(ErrorMessage = "El número de personas es obligatorio")]
        public string Personas { get; set; } 

        public string? Ocasion { get; set; }

        public string? Comentarios { get; set; }
    }
}
