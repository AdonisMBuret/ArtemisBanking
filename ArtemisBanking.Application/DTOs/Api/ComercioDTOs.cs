using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.DTOs.Api
{
    /// <summary>
    /// DTO para crear un nuevo comercio
    /// </summary>
    public class CrearComercioRequestDTO
    {
        [Required(ErrorMessage = "El nombre del comercio es requerido")]
        [MaxLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El RNC es requerido")]
        [MaxLength(20, ErrorMessage = "El RNC no puede exceder 20 caracteres")]
        public string RNC { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para actualizar un comercio existente
    /// </summary>
    public class ActualizarComercioRequestDTO
    {
        [Required(ErrorMessage = "El nombre del comercio es requerido")]
        [MaxLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El RNC es requerido")]
        [MaxLength(20, ErrorMessage = "El RNC no puede exceder 20 caracteres")]
        public string RNC { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para cambiar el estado de un comercio
    /// </summary>
    public class CambiarEstadoComercioRequestDTO
    {
        [Required(ErrorMessage = "El estado es requerido")]
        public bool Status { get; set; }
    }

    /// <summary>
    /// DTO de respuesta para un comercio
    /// </summary>
    public class ComercioResponseDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string RNC { get; set; } = string.Empty;
        public bool EstaActivo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public UsuarioComercioDTO? Usuario { get; set; }
    }

    /// <summary>
    /// DTO simplificado de usuario asociado a comercio
    /// </summary>
    public class UsuarioComercioDTO
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public bool EstaActivo { get; set; }
    }
}
