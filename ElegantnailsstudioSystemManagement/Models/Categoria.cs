using System.ComponentModel.DataAnnotations;

namespace ElegantnailsstudioSystemManagement.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;


        public ICollection<Servicio> Servicios { get; set; } = new List<Servicio>();
    }
}









