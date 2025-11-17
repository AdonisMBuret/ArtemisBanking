using ArtemisBanking.Domain.Entities;

namespace ArtemisBanking.Domain.Interfaces.Repositories
{
    public interface IRepositorioConsumoTarjeta : IRepositorioGenerico<ConsumoTarjeta>
    {
        // Obtener todos los consumos de una tarjeta
        Task<IEnumerable<ConsumoTarjeta>> ObtenerConsumosDeTarjetaAsync(int tarjetaId);
    }
}
