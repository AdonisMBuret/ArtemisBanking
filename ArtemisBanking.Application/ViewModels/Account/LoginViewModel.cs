using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Account
{

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Oye, te olvidaste de poner tu usuario 😅")]
        [Display(Name = "Usuario")]
        [StringLength(50, ErrorMessage = "El usuario no puede tener más de 50 caracteres")]
        public string NombreUsuario { get; set; }

        [Required(ErrorMessage = "Necesitas escribir tu contraseña para entrar")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        [StringLength(100, ErrorMessage = "La contraseña es demasiado larga")]
        public string Contrasena { get; set; }

        [Display(Name = "Mantenerme conectado")]
        public bool Recordarme { get; set; }
    }
}