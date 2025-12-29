using System.ComponentModel.DataAnnotations.Schema;

namespace ElegantnailsstudioSystemManagement.Models
{
    public class Cupo
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Turno { get; set; } = "mañana";
        public int CupoMaximo { get; set; } = 5;
        public int CupoReservado { get; set; } = 0;
        public bool Habilitado { get; set; } = true;
        public DateTime? FechaHabilitacion { get; set; }

        // Propiedades calculadas
        [NotMapped]
        public int CuposDisponibles => CupoMaximo - CupoReservado;

        [NotMapped]
        public string DisplayFecha => Fecha.ToString("dd/MM/yyyy");

    }
}








