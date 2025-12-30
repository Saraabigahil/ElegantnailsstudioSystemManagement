using System.ComponentModel.DataAnnotations;

namespace ElegantnailsstudioSystemManagement.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        
       
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "La contraseña es obligatoria")]
      
       
        public string Password { get; set; } = "";
    }
}











