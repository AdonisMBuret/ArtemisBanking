using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Account
{
  
    public class RestablecerContrasenaViewModel
    {
        [Required]
        public string UsuarioId { get; set; }

        [Required]
        public string Token { get; set; }

        [Required(ErrorMessage = "Necesitas crear una nueva contraseña")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña")]
        public string Contrasena { get; set; }

        [Required(ErrorMessage = "No olvides confirmar tu nueva contraseña")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden. Verifica bien")]
        public string ConfirmarContrasena { get; set; }
    }
}
