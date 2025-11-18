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

        /// <summary>
        /// Obtiene todos los consumos de un comercio
        /// </summary>
        public async Task<IEnumerable<ConsumoTarjeta>> ObtenerConsumosPorComercioAsync(int comercioId)
        {
            return await _context.ConsumosTarjeta
                .Where(c => c.ComercioId == comercioId)
                .OrderByDescending(c => c.FechaConsumo)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene consumos de un comercio en un rango de fechas
        /// </summary>
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

        /// <summary>
        /// Obtiene los consumos más recientes de un comercio
        /// </summary>
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

        /// <summary>
        /// Obtiene consumos paginados de un comercio con filtro de fecha
        /// </summary>
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
