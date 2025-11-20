
namespace ArtemisBanking.Application.DTOs.Api
{
    public class UsuarioComercioDTO
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public bool EstaActivo { get; set; }
    }
}
