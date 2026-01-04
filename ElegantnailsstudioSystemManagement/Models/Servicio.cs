using System.ComponentModel.DataAnnotations;

namespace ElegantnailsstudioSystemManagement.Models
{
    public class Servicio
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(0.01, 10000, ErrorMessage = "El precio debe estar entre 0.01 y 10000")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "La duración es requerida")]
        [Range(5, 480, ErrorMessage = "La duración debe estar entre 5 y 480 minutos")]
        public int DuracionMinutos { get; set; } = 60;

        [Required(ErrorMessage = "La categoría es requerida")]
        public int CategoriaId { get; set; }

        public string ImagenUrl { get; set; } = "/placeholder-image.png";

        public Categoria? Categoria { get; set; }
    }
}
