using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Account
{

    public class ConfirmarCuentaViewModel
    {
        [Required(ErrorMessage = "Necesitamos el ID de usuario")]
        public string UsuarioId { get; set; }

        [Required(ErrorMessage = "No olvides poner el token que te enviamos")]
        [Display(Name = "Token de Confirmación")]
        public string Token { get; set; }
    }
}