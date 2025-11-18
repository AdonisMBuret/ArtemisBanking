using ArtemisBanking.Application.DTOs.Api;
using ArtemisBanking.Application.DTOs;

namespace ArtemisBanking.Application.Interfaces
{
    /// <summary>
    /// Servicio para procesamiento de pagos (Hermes Pay)
    /// </summary>
    public interface IServicioProcesadorPagos
    {
        /// <summary>
        /// Procesa un pago desde un comercio
        /// </summary>
        /// <param name="comercioId">ID del comercio que recibe el pago</param>
        /// <param name="request">Datos del pago a procesar</param>
        /// <returns>Resultado de la operación</returns>
        Task<ResultadoOperacion> ProcesarPagoAsync(int comercioId, ProcesarPagoRequestDTO request);

        /// <summary>
        /// Obtiene las transacciones de un comercio (pagos recibidos)
        /// </summary>
        /// <param name="comercioId">ID del comercio</param>
        /// <param name="page">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <returns>Lista paginada de transacciones</returns>
        Task<PaginatedResponseDTO<TransaccionComercioDTO>> ObtenerTransaccionesComercioAsync(
            int comercioId, 
            int page = 1, 
            int pageSize = 20);
    }
}
