using System.ComponentModel.DataAnnotations;

namespace Restaurant.ViewModels
{
    public class UsuarioViewModel
    {
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        [StringLength(100)]
        public string Correo { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio")]
        public int RolId { get; set; }

        public bool Activo { get; set; } = true;

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string? Password { get; set; }

        // Datos de persona
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string? Nombre { get; set; }

        public string? Apellidos { get; set; }

        public string? Telefono { get; set; }
    }
}
