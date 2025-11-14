using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Web.ViewModels.Account
{
    // ==================== LOGIN ====================

    /// <summary>
    /// ViewModel para el formulario de inicio de sesión
    /// </summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [Display(Name = "Usuario")]
        public string NombreUsuario { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; }

        [Display(Name = "Recordarme")]
        public bool Recordarme { get; set; }
    }

}