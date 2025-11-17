namespace ElegantNailsStudioSystemManagement.Models
{
    public class Cita
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int ServicioId { get; set; }
        public DateTime FechaCita { get; set; }
        public string Turno { get; set; } = "mañana"; 
        public string Estado { get; set; } = "pendiente";
        public string Notas { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

    
        public Usuario? Cliente { get; set; }
        public Servicio? Servicio { get; set; }
    }
}