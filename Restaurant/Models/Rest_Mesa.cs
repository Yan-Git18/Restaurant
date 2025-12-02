using System.ComponentModel.DataAnnotations;

namespace Restaurant.Models
{
    public class Rest_Mesa
    {
        [Key]
        public int MesaId { get; set; }

        [Required(ErrorMessage = "El número de mesa es obligatorio")]
        [Range(1, 20, ErrorMessage = "El número de mesa debe estar entre 1 y 20")]
        public int Numero { get; set; }

        [Required(ErrorMessage = "La capacidad es obligatoria")]
        [Range(1, 20, ErrorMessage = "La capacidad debe estar entre 1 y 20 personas")]
        public int Capacidad { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        [StringLength(20, ErrorMessage = "El estado no puede exceder 20 caracteres")]
        public string Estado { get; set; } = "Libre"; 

        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        public string? Observaciones { get; set; }

        public DateTime FechaCreacion { get; set; } 

        public bool Activo { get; set; } = true;

        public virtual ICollection<Rest_Reserva> Reservas { get; set; } = new List<Rest_Reserva>();
        public virtual ICollection<Rest_Pedido> Pedidos { get; set; } = new List<Rest_Pedido>();
    }
}
