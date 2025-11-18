using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.DTOs.Api
{
    /// <summary>
    /// DTO para confirmar cuenta de usuario
    /// </summary>
    public class ConfirmarCuentaRequestDTO
    {
        [Required(ErrorMessage = "El token es requerido")]
        public string Token { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para solicitar token de reseteo de contraseña
    /// </summary>
    public class SolicitarResetTokenRequestDTO
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        public string UserName { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para resetear contraseña
    /// </summary>
    public class ResetPasswordRequestDTO
    {
        [Required(ErrorMessage = "El ID de usuario es requerido")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El token es requerido")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmación de contraseña es requerida")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
