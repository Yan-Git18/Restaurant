using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    public class Rest_Producto
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        public string Nombre { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, 9999.99, ErrorMessage = "El precio debe estar entre 0.01 y 9999.99")]
        [Column(TypeName = "decimal(10,2)")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal Precio { get; set; }

        [StringLength(200, ErrorMessage = "La URL de la imagen no puede exceder 200 caracteres")]
        [Display(Name = "Imagen")]
        public string? ImagenUrl { get; set; }

        [Display(Name = "Disponible")]
        public bool Disponible { get; set; } = true;

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria")]
        [Display(Name = "Categoría")]
        public int CategoriaId { get; set; }

        [Required(ErrorMessage = "El inventario es obligatorio")]
        [Display(Name = "Inventario")]
        public int InventarioId { get; set; }

        // Stock ahora está aquí
        [Required(ErrorMessage = "El stock es obligatorio")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public int Stock { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        // Navegación
        [ForeignKey("CategoriaId")]
        public virtual Rest_Categoria Categoria { get; set; }

        [ForeignKey("InventarioId")]
        public virtual Rest_Inventario Inventario { get; set; }

        public virtual ICollection<Rest_DetallePedido> DetallesPedido { get; set; } = new List<Rest_DetallePedido>();
    }
}
