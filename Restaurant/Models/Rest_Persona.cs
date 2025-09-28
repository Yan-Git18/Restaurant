using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    public class Rest_Persona
    {
        [Key]
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        public string Nombre { get; set; }

        [StringLength(100, ErrorMessage = "Los apellidos no pueden exceder 100 caracteres")]
        public string? Apellidos { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        [StringLength(9, ErrorMessage = "El teléfono no puede exceder 9 caracteres")]
        public string Telefono { get; set; }

        //[Required(ErrorMessage = "El correo es obligatorio")]
        //[EmailAddress(ErrorMessage = "Formato de correo inválido")]
        //[StringLength(100, ErrorMessage = "El correo no puede exceder 100 caracteres")]
        //public string Correo { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
        public string? Direccion { get; set; }

        [Display(Name = "Fecha de Registro")]
        public DateTime FechaRegistro { get; set; }

        [Display(Name = "Usuario ID")]
        public int? UsuarioId { get; set; }


        [ForeignKey("UsuarioId")]
        public virtual Rest_Usuario? Usuario { get; set; }

        public virtual ICollection<Rest_Reserva> Reservas { get; set; } = new List<Rest_Reserva>();
        public virtual ICollection<Rest_Pedido> Pedidos { get; set; } = new List<Rest_Pedido>();
    }
}
