
using System.ComponentModel.DataAnnotations;


namespace Restaurant.Models
{
    public class Rest_Rol
    {
        [Key]
        public int RolId { get; set; }

        [Required(ErrorMessage = "El nombre del rol es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
        public string Nombre { get; set; }

        [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
        public string? Descripcion { get; set; }

        public virtual ICollection<Rest_Usuario> Usuarios { get; set; } = new List<Rest_Usuario>();
    }
}
