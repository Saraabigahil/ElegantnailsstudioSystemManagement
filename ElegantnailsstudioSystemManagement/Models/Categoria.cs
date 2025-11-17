namespace ElegantNailsStudioSystemManagement.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;

        // Navigation properties
        public List<Servicio> Servicios { get; set; } = new List<Servicio>();
    }
}