namespace ElegantNailsStudioSystemManagement.Models
{
    public class Cupo
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Turno { get; set; } = string.Empty; 
        public int CupoMaximo { get; set; } = 5;
        public int CupoActual { get; set; } = 0;

        public bool TieneCupoDisponible => CupoActual < CupoMaximo;
    }
}