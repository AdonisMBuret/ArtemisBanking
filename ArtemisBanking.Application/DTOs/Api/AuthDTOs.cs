using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.DTOs.Api
{
    /// <summary>
    /// DTO para la solicitud de login en el API
    /// </summary>
    public class LoginRequestDTO
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para la respuesta de login exitoso
    /// </summary>
    public class LoginResponseDTO
    {
        public string Jwt { get; set; } = string.Empty;
    }
}
