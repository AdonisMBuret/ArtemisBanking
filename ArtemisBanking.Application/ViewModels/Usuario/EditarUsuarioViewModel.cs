using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Usuario
{
    /// <summary>
    /// ViewModel para editar un usuario existente
    /// Incluye validaciones con mensajes amigables
    /// </summary>
    public class EditarUsuarioViewModel
    {
        [Required]
        public string Id { get; set; }

        [Required(ErrorMessage = "El nombre no puede estar vacío")]
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

        [Required(ErrorMessage = "El correo electrónico es necesario")]
        [EmailAddress(ErrorMessage = "Ese email no se ve bien. Revísalo 📧")]
        [Display(Name = "Correo Electrónico")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres")]
        [Display(Name = "Nombre de Usuario")]
        public string NombreUsuario { get; set; }

        [StringLength(100, MinimumLength = 8, ErrorMessage = "Si cambias la contraseña, debe tener al menos 8 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña (dejar en blanco si no deseas cambiarla)")]
        public string NuevaContrasena { get; set; }

        [Display(Name = "Tipo de Usuario")]
        public string TipoUsuario { get; set; }

        [Range(0, 1000000, ErrorMessage = "El monto debe estar entre RD$0 y RD$1,000,000")]
        [Display(Name = "Monto Adicional (solo para clientes)")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal MontoAdicional { get; set; } = 0;
    }
}