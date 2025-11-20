using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.DTOs.Api
{

    public class LoginRequestDTO
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        public string Password { get; set; } = string.Empty;
    }

}
