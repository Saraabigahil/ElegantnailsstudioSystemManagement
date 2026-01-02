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
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaCancelacion { get; set; }
        public DateTime? FechaConfirmacion { get; set; }

        public virtual Usuario? Cliente { get; set; }
        public virtual Servicio? Servicio { get; set; }
    }
}






