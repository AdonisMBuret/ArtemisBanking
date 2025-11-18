using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Usuario
{
    /// <summary>
    /// ViewModel para crear un nuevo usuario
    /// Incluye validaciones completas con mensajes amigables
    /// </summary>
    public class CrearUsuarioViewModel
    {
        [Required(ErrorMessage = "No te olvides del nombre")]
        [StringLength(100, ErrorMessage = "El nombre es muy largo, máximo 100 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Necesitamos el apellido")]
        [StringLength(100, ErrorMessage = "El apellido es muy largo, máximo 100 caracteres")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "La cédula es obligatoria")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "La cédula debe tener 11 dígitos sin guiones")]
        [Display(Name = "Cédula")]
        public string Cedula { get; set; }

        [Required(ErrorMessage = "Necesitamos un correo electrónico")]
        [EmailAddress(ErrorMessage = "Ese email no se ve bien. Revísalo 📧")]
        [Display(Name = "Correo Electrónico")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "Tienes que crear un nombre de usuario")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres")]
        [Display(Name = "Nombre de Usuario")]
        public string NombreUsuario { get; set; }

        [Required(ErrorMessage = "No olvides crear una contraseña")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; }

        [Required(ErrorMessage = "Confirma tu contraseña para estar seguro")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden. Verifica bien")]
        public string ConfirmarContrasena { get; set; }

        [Required(ErrorMessage = "Debes elegir qué tipo de usuario es")]
        [Display(Name = "Tipo de Usuario")]
        public string TipoUsuario { get; set; }

        [Range(0, 1000000, ErrorMessage = "El monto debe estar entre RD$0 y RD$1,000,000")]
        [Display(Name = "Monto Inicial (Opcional)")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal MontoInicial { get; set; } = 0;
    }
}
