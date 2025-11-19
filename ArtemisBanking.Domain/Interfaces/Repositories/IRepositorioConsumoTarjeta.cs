using ArtemisBanking.Domain.Entities;

namespace ArtemisBanking.Domain.Interfaces.Repositories
{
    public interface IRepositorioConsumoTarjeta : IRepositorioGenerico<ConsumoTarjeta>
    {

        Task<IEnumerable<ConsumoTarjeta>> ObtenerConsumosDeTarjetaAsync(int tarjetaId);

        Task<IEnumerable<ConsumoTarjeta>> ObtenerConsumosPorComercioAsync(int comercioId);

        Task<IEnumerable<ConsumoTarjeta>> ObtenerConsumosPorComercioYFechaAsync(
            int comercioId, 
            DateTime fechaInicio, 
            DateTime fechaFin);

        Task<IEnumerable<ConsumoTarjeta>> ObtenerConsumosRecientesDeComercioAsync(
            int comercioId, 
            int cantidad);

        Task<(IEnumerable<ConsumoTarjeta> Consumos, int TotalRegistros)> ObtenerConsumosPaginadosDeComercioAsync(
            int comercioId,
            int pagina,
            int registrosPorPagina,
            DateTime fechaInicio,
            DateTime fechaFin);
    }
}
