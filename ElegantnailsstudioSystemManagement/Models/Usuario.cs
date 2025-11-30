using System.ComponentModel.DataAnnotations;

namespace ElegantnailsstudioSystemManagement.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public int Rol { get; set; }
        public int RolId { get; set; }
    }
}