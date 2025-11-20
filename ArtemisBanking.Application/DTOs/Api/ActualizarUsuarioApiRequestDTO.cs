using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.DTOs.Api
{
    public class ActualizarUsuarioApiRequestDTO
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

        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        public string? Password { get; set; }
        public decimal? MontoAdicional { get; set; }
    }
}
