namespace Restaurant.Models
{
    public class Rest_InventarioViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public string UnidadMedida { get; set; }
        public int StockTotal { get; set; }   // 👈 suma de los productos
        public int StockMinimo { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}
