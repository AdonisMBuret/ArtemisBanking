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

        public async Task<IEnumerable<ConsumoTarjeta>> ObtenerConsumosDeTarjetaAsync(int tarjetaId)
        {
            return await _context.ConsumosTarjeta
                .Where(c => c.TarjetaId == tarjetaId)
                .OrderByDescending(c => c.FechaConsumo)
                .ToListAsync();
        }

         public async Task<IEnumerable<ConsumoTarjeta>> ObtenerConsumosPorComercioAsync(int comercioId)
        {
            return await _context.ConsumosTarjeta
                .Where(c => c.ComercioId == comercioId)
                .OrderByDescending(c => c.FechaConsumo)
                .ToListAsync();
        }

         public async Task<IEnumerable<ConsumoTarjeta>> ObtenerConsumosPorComercioYFechaAsync(
            int comercioId,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            return await _context.ConsumosTarjeta
                .Where(c => c.ComercioId == comercioId 
                    && c.FechaConsumo >= fechaInicio 
                    && c.FechaConsumo <= fechaFin)
                .OrderByDescending(c => c.FechaConsumo)
                .ToListAsync();
        }

          public async Task<IEnumerable<ConsumoTarjeta>> ObtenerConsumosRecientesDeComercioAsync(
            int comercioId,
            int cantidad)
        {
            return await _context.ConsumosTarjeta
                .Include(c => c.Tarjeta)
                .Where(c => c.ComercioId == comercioId)
                .OrderByDescending(c => c.FechaConsumo)
                .Take(cantidad)
                .ToListAsync();
        }

         public async Task<(IEnumerable<ConsumoTarjeta> Consumos, int TotalRegistros)> ObtenerConsumosPaginadosDeComercioAsync(
            int comercioId,
            int pagina,
            int registrosPorPagina,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            var query = _context.ConsumosTarjeta
                .Include(c => c.Tarjeta)
                    .ThenInclude(t => t.Cliente)
                .Where(c => c.ComercioId == comercioId
                    && c.FechaConsumo >= fechaInicio
                    && c.FechaConsumo <= fechaFin)
                .OrderByDescending(c => c.FechaConsumo);

            var totalRegistros = await query.CountAsync();

            var consumos = await query
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();

            return (consumos, totalRegistros);
        }
    }
}
