using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    public class Rest_Usuario
    {
        [Key]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        [StringLength(100, ErrorMessage = "El correo no puede exceder 100 caracteres")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(255, ErrorMessage = "El hash de contraseña no puede exceder 255 caracteres")]
        public string PasswordHash { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio")]
        public int RolId { get; set; }

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } 

        [Display(Name = "Último Acceso")]
        public DateTime? UltimoAcceso { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;


        [ForeignKey("RolId")]
        public virtual Rest_Rol Rol { get; set; }

        public virtual Rest_Persona? Cliente { get; set; }
    }
}
