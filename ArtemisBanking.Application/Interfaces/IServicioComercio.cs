using ArtemisBanking.Application.DTOs.Api;

namespace ArtemisBanking.Application.Interfaces
{
    /// <summary>
    /// Servicio para gestión de comercios
    /// </summary>
    public interface IServicioComercio
    {
        /// <summary>
        /// Obtiene todos los comercios paginados
        /// </summary>
        Task<PaginatedResponseDTO<ComercioResponseDTO>> ObtenerComerciosPaginadosAsync(int page = 1, int pageSize = 20);

        /// <summary>
        /// Obtiene todos los comercios activos (sin paginación)
        /// </summary>
        Task<IEnumerable<ComercioResponseDTO>> ObtenerComerciosActivosAsync();

        /// <summary>
        /// Obtiene un comercio por su ID
        /// </summary>
        Task<ComercioResponseDTO?> ObtenerComercioPorIdAsync(int id);

        /// <summary>
        /// Crea un nuevo comercio
        /// </summary>
        Task<ComercioResponseDTO> CrearComercioAsync(CrearComercioRequestDTO request);

        /// <summary>
        /// Actualiza un comercio existente
        /// </summary>
        Task<bool> ActualizarComercioAsync(int id, ActualizarComercioRequestDTO request);

        /// <summary>
        /// Cambia el estado de un comercio (activo/inactivo)
        /// </summary>
        Task<bool> CambiarEstadoComercioAsync(int id, bool nuevoEstado);

        /// <summary>
        /// Verifica si un comercio ya tiene un usuario asociado
        /// </summary>
        Task<bool> TieneUsuarioAsociadoAsync(int comercioId);
    }
}
