using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.DTOs
{
    public class ActualizarUsuarioDTO
    {
        [Required]
        public string UsuarioId { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        public string Apellido { get; set; }

        [Required]
        public string Cedula { get; set; }

        [Required]
        [EmailAddress]
        public string Correo { get; set; }

        [Required]
        public string NombreUsuario { get; set; }

        public string NuevaContrasena { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MontoAdicional { get; set; } = 0;
    }
}