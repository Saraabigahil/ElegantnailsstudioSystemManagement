namespace ElegantNailsStudioSystemManagement.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public int RolId { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.Now;


        public Rol? Rol { get; set; }
    }
}