using ArtemisBanking.Domain.Entities;

namespace ArtemisBanking.Domain.Interfaces.Repositories
{
    public interface IRepositorioConsumoTarjeta : IRepositorioGenerico<ConsumoTarjeta>
    {
        /// <summary>
        /// Obtiene todos los consumos de una tarjeta
        /// </summary>
        Task<IEnumerable<ConsumoTarjeta>> ObtenerConsumosDeTarjetaAsync(int tarjetaId);

        /// <summary>
        /// Obtiene todos los consumos de un comercio
        /// </summary>
        Task<IEnumerable<ConsumoTarjeta>> ObtenerConsumosPorComercioAsync(int comercioId);

        /// <summary>
        /// Obtiene consumos de un comercio en un rango de fechas
        /// </summary>
        Task<IEnumerable<ConsumoTarjeta>> ObtenerConsumosPorComercioYFechaAsync(
            int comercioId, 
            DateTime fechaInicio, 
            DateTime fechaFin);

        /// <summary>
        /// Obtiene los consumos más recientes de un comercio
        /// </summary>
        Task<IEnumerable<ConsumoTarjeta>> ObtenerConsumosRecientesDeComercioAsync(
            int comercioId, 
            int cantidad);

        /// <summary>
        /// Obtiene consumos paginados de un comercio con filtro de fecha
        /// </summary>
        Task<(IEnumerable<ConsumoTarjeta> Consumos, int TotalRegistros)> ObtenerConsumosPaginadosDeComercioAsync(
            int comercioId,
            int pagina,
            int registrosPorPagina,
            DateTime fechaInicio,
            DateTime fechaFin);
    }
}
