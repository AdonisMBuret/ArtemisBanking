

namespace ArtemisBanking.Application.DTOs.Api
{
    public class UsuarioApiResponseDTO
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string TipoUsuario { get; set; } = string.Empty;
        public bool EstaActivo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
