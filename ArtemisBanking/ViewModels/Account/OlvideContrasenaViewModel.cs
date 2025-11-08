using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Web.ViewModels.Account
{
    /// <summary>
    /// ViewModel para solicitar reseteo de contraseña
    /// </summary>
    public class OlvideContrasenaViewModel
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [Display(Name = "Nombre de Usuario")]
        public string NombreUsuario { get; set; }
    }
}
