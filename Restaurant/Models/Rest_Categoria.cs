using System.ComponentModel.DataAnnotations;

namespace Restaurant.Models
{
    public class Rest_Categoria
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la categoría es obligatorio")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres")]
        public string Nombre { get; set; }

        [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
        public string? Descripcion { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        [Display(Name = "Orden")]
        public int? Orden { get; set; }

        // Navegación
        public virtual ICollection<Rest_Producto> Productos { get; set; } = new List<Rest_Producto>();
    }
}
