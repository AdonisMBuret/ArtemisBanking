using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Web.ViewModels.Account
{
   
// ==================== CONFIRMACIÓN DE CUENTA ====================

    /// <summary>
    /// ViewModel para confirmar la cuenta con el token enviado por correo
    /// </summary>
    public class ConfirmarCuentaViewModel
    {
        [Required(ErrorMessage = "El ID de usuario es obligatorio")]
        public string UsuarioId { get; set; }

        [Required(ErrorMessage = "El token es obligatorio")]
        [Display(Name = "Token de Confirmación")]
        public string Token { get; set; }
    }
}