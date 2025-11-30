namespace ElegantnailsstudioSystemManagement.Models
{
    public class Servicio
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int DuracionMinutos { get; set; } = 60;
        public int CategoriaId { get; set; }
        public string ImagenUrl { get; set; } = string.Empty;
    }
}