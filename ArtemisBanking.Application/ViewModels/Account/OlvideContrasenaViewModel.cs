using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Account
{
    // ==================== OLVIDE CONTRASEÑA ====================

    /// ViewModel para solicitar el reseteo de contraseña
    public class OlvideContrasenaViewModel
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [Display(Name = "Nombre de Usuario")]
        public string NombreUsuario { get; set; }
    }
}
