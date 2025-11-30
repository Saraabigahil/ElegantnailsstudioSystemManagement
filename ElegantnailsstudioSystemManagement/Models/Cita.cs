namespace ElegantnailsstudioSystemManagement.Models
{
    public class Cita
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int ServicioId { get; set; }
        public DateTime FechaCita { get; set; }
        public string Turno { get; set; } = "mañana";
        public string Estado { get; set; } = "pendiente";
    }
}