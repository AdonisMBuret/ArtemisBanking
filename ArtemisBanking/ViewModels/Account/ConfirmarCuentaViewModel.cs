using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Web.ViewModels.Account
{
    /// <summary>
    /// ViewModel para confirmar la cuenta con token
    /// </summary>
    public class ConfirmarCuentaViewModel
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string UsuarioId { get; set; }
    }
}
