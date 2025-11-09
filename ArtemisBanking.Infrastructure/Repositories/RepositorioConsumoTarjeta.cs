using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Infrastructure.Repositories
{
    public class RepositorioConsumoTarjeta : RepositorioGenerico<ConsumoTarjeta>, IRepositorioConsumoTarjeta
    {
        public RepositorioConsumoTarjeta(ArtemisBankingDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Obtiene todos los consumos de una tarjeta ordenados por fecha
        /// </summary>
        public async Task<IEnumerable<ConsumoTarjeta>> ObtenerConsumosDeTarjetaAsync(int tarjetaId)
        {
            return await _context.ConsumosTarjeta
                .Where(c => c.TarjetaId == tarjetaId)
                .OrderByDescending(c => c.FechaConsumo)
                .ToListAsync();
        }
    }
}
