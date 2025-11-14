using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Web.ViewModels.Usuario
{
    /// <summary>
    /// ViewModel para editar un usuario existente
    /// </summary>
    // ==================== EDITAR USUARIO ====================

    /// <summary>
    /// ViewModel para editar un usuario existente
    /// </summary>
    public class EditarUsuarioViewModel
    {
        [Required]
        public string Id { get; set; }

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

        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña (dejar en blanco si no desea cambiarla)")]
        public string NuevaContrasena { get; set; }

        [Display(Name = "Tipo de Usuario")]
        public string TipoUsuario { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El monto debe ser mayor o igual a 0")]
        [Display(Name = "Monto Adicional (solo para clientes)")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal MontoAdicional { get; set; } = 0;
    }
}