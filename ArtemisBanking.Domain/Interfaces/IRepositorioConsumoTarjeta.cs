using ArtemisBanking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Domain.Interfaces
{
    public interface IRepositorioConsumoTarjeta : IRepositorioGenerico<ConsumoTarjeta>
    {
        // Obtener todos los consumos de una tarjeta
        Task<IEnumerable<ConsumoTarjeta>> ObtenerConsumosDeTarjetaAsync(int tarjetaId);
    }
}
