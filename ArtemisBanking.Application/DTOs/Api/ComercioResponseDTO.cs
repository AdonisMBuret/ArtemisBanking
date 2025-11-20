
namespace ArtemisBanking.Application.DTOs.Api
{
    public class ComercioResponseDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string RNC { get; set; } = string.Empty;
        public bool EstaActivo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public UsuarioComercioDTO? Usuario { get; set; }
    }
}
