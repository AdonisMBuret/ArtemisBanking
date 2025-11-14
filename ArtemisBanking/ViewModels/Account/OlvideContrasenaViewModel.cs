using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Web.ViewModels.Account
{
    // ==================== OLVIDE CONTRASEÑA ====================

    /// <summary>
    /// ViewModel para solicitar el reseteo de contraseña
    /// </summary>
    public class OlvideContrasenaViewModel
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [Display(Name = "Nombre de Usuario")]
        public string NombreUsuario { get; set; }
    }
}
