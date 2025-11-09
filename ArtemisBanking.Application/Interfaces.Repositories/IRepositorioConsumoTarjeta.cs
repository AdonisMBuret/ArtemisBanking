using ArtemisBanking.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtemisBanking.Application.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz para operaciones con consumos de tarjetas
    /// </summary>
    public interface IRepositorioConsumoTarjeta : IRepositorioGenerico<ConsumoTarjeta>
    {
        // Obtener todos los consumos de una tarjeta
        Task<IEnumerable<ConsumoTarjeta>> ObtenerConsumosDeTarjetaAsync(int tarjetaId);
    }
}