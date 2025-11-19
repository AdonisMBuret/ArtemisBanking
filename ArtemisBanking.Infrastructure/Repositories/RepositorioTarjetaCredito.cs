using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ArtemisBanking.Domain.Interfaces.Repositories;

namespace ArtemisBanking.Infrastructure.Repositories
{
    public class RepositorioTarjetaCredito : RepositorioGenerico<TarjetaCredito>, IRepositorioTarjetaCredito
    {
        public RepositorioTarjetaCredito(ArtemisBankingDbContext context) : base(context)
        {
        }


        public override async Task<TarjetaCredito> ObtenerPorIdAsync(int id)
        {
            return await _context.TarjetasCredito
                .Include(t => t.Cliente)
                .Include(t => t.Consumos)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<TarjetaCredito> ObtenerPorNumeroTarjetaAsync(string numeroTarjeta)
        {
            return await _context.TarjetasCredito
                .Include(t => t.Cliente)
                .Include(t => t.Consumos)
                .FirstOrDefaultAsync(t => t.NumeroTarjeta == numeroTarjeta);
        }


        public async Task<IEnumerable<TarjetaCredito>> ObtenerTarjetasDeUsuarioAsync(string usuarioId)
        {
            return await _context.TarjetasCredito
                .Include(t => t.Consumos)
                .Where(t => t.ClienteId == usuarioId)
                .OrderByDescending(t => t.EstaActiva)
                .ThenByDescending(t => t.FechaCreacion)
                .ToListAsync();
        }

         
        public async Task<IEnumerable<TarjetaCredito>> ObtenerTarjetasActivasDeUsuarioAsync(string usuarioId)
        {
            return await _context.TarjetasCredito
                .Where(t => t.ClienteId == usuarioId && t.EstaActiva)
                .OrderByDescending(t => t.FechaCreacion)
                .ToListAsync();
        }
         
        public async Task<(IEnumerable<TarjetaCredito> tarjetas, int total)> ObtenerTarjetasPaginadasAsync(
            int pagina,
            int tamano,
            string cedula = null,
            bool? estaActiva = null)
        {
            var query = _context.TarjetasCredito
                .Include(t => t.Cliente)
                .AsQueryable();

            if (!string.IsNullOrEmpty(cedula))
            {
                query = query.Where(t => t.Cliente.Cedula == cedula);
            }

            if (estaActiva.HasValue)
            {
                query = query.Where(t => t.EstaActiva == estaActiva.Value);
            }

            var total = await query.CountAsync();

            var tarjetas = await query
                .OrderByDescending(t => t.FechaCreacion)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();

            return (tarjetas, total);
        }

                 
        public async Task<string> GenerarNumeroTarjetaUnicoAsync()
        {
            string numeroTarjeta;
            var random = new Random();

            do
            {
                numeroTarjeta = "";
                for (int i = 0; i < 16; i++)
                {
                    numeroTarjeta += random.Next(0, 10).ToString();
                }
            }
            while (await ExisteNumeroTarjetaAsync(numeroTarjeta));

            return numeroTarjeta;
        }

                 
        public async Task<bool> ExisteNumeroTarjetaAsync(string numeroTarjeta)
        {
            return await _context.TarjetasCredito
                .AnyAsync(t => t.NumeroTarjeta == numeroTarjeta);
        }

         
        public async Task<int> ContarTarjetasActivasAsync()
        {
            return await _context.TarjetasCredito
                .CountAsync(t => t.EstaActiva);
        }
    }
}
