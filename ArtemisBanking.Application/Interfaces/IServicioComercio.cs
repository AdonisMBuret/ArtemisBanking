using ArtemisBanking.Application.DTOs.Api;

namespace ArtemisBanking.Application.Interfaces
{

    public interface IServicioComercio
    {
        Task<PaginatedResponseDTO<ComercioResponseDTO>> ObtenerComerciosPaginadosAsync(int page = 1, int pageSize = 20);

        Task<IEnumerable<ComercioResponseDTO>> ObtenerComerciosActivosAsync();
         
        Task<ComercioResponseDTO?> ObtenerComercioPorIdAsync(int id);
         
        Task<ComercioResponseDTO> CrearComercioAsync(CrearComercioRequestDTO request);

        Task<bool> ActualizarComercioAsync(int id, ActualizarComercioRequestDTO request);

        Task<bool> CambiarEstadoComercioAsync(int id, bool nuevoEstado);

        Task<bool> TieneUsuarioAsociadoAsync(int comercioId);
    }
}
