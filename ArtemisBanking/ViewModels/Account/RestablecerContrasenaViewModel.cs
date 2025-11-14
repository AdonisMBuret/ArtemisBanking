using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Web.ViewModels.Account
{
    // ==================== RESTABLECER CONTRASEÑA ====================

    /// <summary>
    /// ViewModel para restablecer la contraseña con el token recibido
    /// </summary>
    public class RestablecerContrasenaViewModel
    {
        [Required]
        public string UsuarioId { get; set; }

        [Required]
        public string Token { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} caracteres", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña")]
        public string Contrasena { get; set; }

        [Required(ErrorMessage = "Debe confirmar la contraseña")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmarContrasena { get; set; }
    }
}
