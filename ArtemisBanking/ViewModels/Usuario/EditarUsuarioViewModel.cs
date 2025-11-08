using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Web.ViewModels.Usuario
{
    /// <summary>
    /// ViewModel para editar un usuario existente
    /// </summary>
    public class EditarUsuarioViewModel
    {
        [Required]
        public string Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(100)]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "La cédula es requerida")]
        [StringLength(20)]
        [Display(Name = "Cédula")]
        public string Cedula { get; set; }

        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "El correo no es válido")]
        [Display(Name = "Correo Electrónico")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(50)]
        [Display(Name = "Nombre de Usuario")]
        public string NombreUsuario { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña (opcional)")]
        public string NuevaContrasena { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Nueva Contraseña")]
        [Compare("NuevaContrasena", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmarNuevaContrasena { get; set; }

        // Solo para clientes
        [Display(Name = "Monto Adicional")]
        [Range(0, double.MaxValue, ErrorMessage = "El monto adicional debe ser mayor o igual a 0")]
        public decimal MontoAdicional { get; set; } = 0;

        public string TipoUsuario { get; set; }
    }
}