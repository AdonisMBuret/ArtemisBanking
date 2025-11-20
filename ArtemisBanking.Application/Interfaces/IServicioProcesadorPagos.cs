using ArtemisBanking.Application.DTOs.Api;
using ArtemisBanking.Application.DTOs;

namespace ArtemisBanking.Application.Interfaces
{

    public interface IServicioProcesadorPagos
    {

        Task<ResultadoOperacion> ProcesarPagoAsync(int comercioId, ProcesarPagoRequestDTO request);

        Task<PaginatedResponseDTO<TransaccionComercioDTO>> ObtenerTransaccionesComercioAsync(
            int comercioId, 
            int page = 1, 
            int pageSize = 20);
    }
}
