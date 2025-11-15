using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Usuario
{
    // ==================== CREAR USUARIO ====================

    /// <summary>
    /// ViewModel para crear un nuevo usuario
    /// </summary>
    public class CrearUsuarioViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(100)]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "La cédula es obligatoria")]
        [StringLength(20)]
        [Display(Name = "Cédula")]
        public string Cedula { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo no es válido")]
        [Display(Name = "Correo Electrónico")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [StringLength(50)]
        [Display(Name = "Nombre de Usuario")]
        public string NombreUsuario { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} caracteres", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; }

        [Required(ErrorMessage = "Debe confirmar la contraseña")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmarContrasena { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un tipo de usuario")]
        [Display(Name = "Tipo de Usuario")]
        public string TipoUsuario { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El monto debe ser mayor o igual a 0")]
        [Display(Name = "Monto Inicial")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal MontoInicial { get; set; } = 0;
    }
}
