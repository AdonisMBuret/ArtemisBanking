using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ArtemisBanking.Domain.Interfaces.Repositories;

namespace ArtemisBanking.Infrastructure.Repositories
{
    public class RepositorioConsumoTarjeta : RepositorioGenerico<ConsumoTarjeta>, IRepositorioConsumoTarjeta
    {
        public RepositorioConsumoTarjeta(ArtemisBankingDbContext context) : base(context)
        {
        }

        /// Obtiene todos los consumos de una tarjeta ordenados por fecha
        public async Task<IEnumerable<ConsumoTarjeta>> ObtenerConsumosDeTarjetaAsync(int tarjetaId)
        {
            return await _context.ConsumosTarjeta
                .Where(c => c.TarjetaId == tarjetaId)
                .OrderByDescending(c => c.FechaConsumo)
                .ToListAsync();
        }
    }
}
