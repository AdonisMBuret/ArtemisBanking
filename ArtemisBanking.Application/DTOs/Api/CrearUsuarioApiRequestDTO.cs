using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.DTOs.Api
{
    public class CrearUsuarioApiRequestDTO
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [MaxLength(100)]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cédula es requerida")]
        [MaxLength(20)]
        public string Cedula { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "El correo no es válido")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [MaxLength(50)]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmación de contraseña es requerida")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de usuario es requerido")]
        [RegularExpression("^(Administrador|Cajero|Cliente)$", ErrorMessage = "Tipo de usuario inválido")]
        public string TipoUsuario { get; set; } = string.Empty;


        public decimal? MontoInicial { get; set; }
    }


}
