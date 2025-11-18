using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Account
{
    /// <summary>
    /// ViewModel para solicitar el reseteo de contraseña
    /// Incluye validaciones con mensajes amigables
    /// </summary>
    public class OlvideContrasenaViewModel
    {
        [Required(ErrorMessage = "Oye, necesitamos tu nombre de usuario para ayudarte")]
        [Display(Name = "Nombre de Usuario")]
        [StringLength(50, ErrorMessage = "El usuario no puede tener más de 50 caracteres")]
        public string NombreUsuario { get; set; }
    }
}
