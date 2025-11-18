using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Account
{
    /// <summary>
    /// ViewModel para confirmar la cuenta con el token enviado por correo
    /// Incluye validaciones con mensajes amigables
    /// </summary>
    public class ConfirmarCuentaViewModel
    {
        [Required(ErrorMessage = "Necesitamos el ID de usuario")]
        public string UsuarioId { get; set; }

        [Required(ErrorMessage = "No olvides poner el token que te enviamos")]
        [Display(Name = "Token de Confirmación")]
        public string Token { get; set; }
    }
}