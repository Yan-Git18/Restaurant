using System.ComponentModel.DataAnnotations;

namespace Restaurant.Models
{
    public class Rest_Inventario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        public string Nombre { get; set; }

        [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "La unidad de medida es obligatoria")]
        [StringLength(20, ErrorMessage = "La unidad de medida no puede exceder 20 caracteres")]
        [Display(Name = "Unidad de Medida")]
        public string UnidadMedida { get; set; }

        [Required(ErrorMessage = "El stock es obligatorio")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public int Stock { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo")]
        [Display(Name = "Stock Mínimo")]
        public int StockMinimo { get; set; } = 0;

        [Display(Name = "Fecha de Actualización")]
        public DateTime FechaActualizacion { get; set; } //= DateTime.Now;

        // Navegación
        public virtual ICollection<Rest_Producto> Productos { get; set; } = new List<Rest_Producto>();
    }
}
