using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Comercio
{
    /// <summary>
    /// ViewModel para asignar un usuario a un comercio
    /// </summary>
    public class AsignarUsuarioComercioViewModel
    {
        public int ComercioId { get; set; }
        public string NombreComercio { get; set; } = string.Empty;
        public string RNC { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre del Usuario")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cédula es obligatoria")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "La cédula debe tener 11 dígitos")]
        [Display(Name = "Cédula")]
        public string Cedula { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "Correo Electrónico")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Entre 3 y 50 caracteres")]
        [Display(Name = "Nombre de Usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mínimo 8 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe confirmar la contraseña")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmarContrasena { get; set; } = string.Empty;
    }
}
