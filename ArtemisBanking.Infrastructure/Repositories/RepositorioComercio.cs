using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Domain.Interfaces.Repositories;
using ArtemisBanking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Infrastructure.Repositories
{
    public class RepositorioComercio : RepositorioGenerico<Comercio>, IRepositorioComercio
    {
        public RepositorioComercio(ArtemisBankingDbContext context) : base(context)
        {
        }

        public async Task<Comercio?> ObtenerPorRNCAsync(string rnc)
        {
            return await _context.Comercios
                .FirstOrDefaultAsync(c => c.RNC == rnc);
        }

        public async Task<IEnumerable<Comercio>> ObtenerActivosAsync()
        {
            return await _context.Comercios
                .Where(c => c.EstaActivo)
                .OrderByDescending(c => c.FechaCreacion)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Comercio> Comercios, int TotalRegistros)> ObtenerPaginadoAsync(int pagina, int registrosPorPagina)
        {
            var query = _context.Comercios
                .Include(c => c.Usuario)
                .OrderByDescending(c => c.FechaCreacion);

            var totalRegistros = await query.CountAsync();

            var comercios = await query
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();

            return (comercios, totalRegistros);
        }

        public async Task<Comercio?> ObtenerConUsuarioAsync(int id)
        {
            return await _context.Comercios
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> TieneUsuarioAsociadoAsync(int comercioId)
        {
            return await _context.Comercios
                .AnyAsync(c => c.Id == comercioId && c.UsuarioId != null);
        }
    }
}
