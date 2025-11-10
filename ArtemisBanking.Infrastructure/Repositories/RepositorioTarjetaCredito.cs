using ArtemisBanking.Domain.Interfaces;
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
    public class RepositorioTarjetaCredito : RepositorioGenerico<TarjetaCredito>, IRepositorioTarjetaCredito
    {
        public RepositorioTarjetaCredito(ArtemisBankingDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Busca una tarjeta por su número de 16 dígitos
        /// </summary>
        public async Task<TarjetaCredito> ObtenerPorNumeroTarjetaAsync(string numeroTarjeta)
        {
            return await _context.TarjetasCredito
                .Include(t => t.Cliente)
                .Include(t => t.Consumos)
                .FirstOrDefaultAsync(t => t.NumeroTarjeta == numeroTarjeta);
        }

        /// <summary>
        /// Obtiene todas las tarjetas de un usuario
        /// </summary>
        public async Task<IEnumerable<TarjetaCredito>> ObtenerTarjetasDeUsuarioAsync(string usuarioId)
        {
            return await _context.TarjetasCredito
                .Include(t => t.Consumos)
                .Where(t => t.ClienteId == usuarioId)
                .OrderByDescending(t => t.EstaActiva)
                .ThenByDescending(t => t.FechaCreacion)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene solo las tarjetas activas de un usuario
        /// </summary>
        public async Task<IEnumerable<TarjetaCredito>> ObtenerTarjetasActivasDeUsuarioAsync(string usuarioId)
        {
            return await _context.TarjetasCredito
                .Where(t => t.ClienteId == usuarioId && t.EstaActiva)
                .OrderByDescending(t => t.FechaCreacion)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene tarjetas paginadas con filtros opcionales
        /// </summary>
        public async Task<(IEnumerable<TarjetaCredito> tarjetas, int total)> ObtenerTarjetasPaginadasAsync(
            int pagina,
            int tamano,
            string cedula = null,
            bool? estaActiva = null)
        {
            var query = _context.TarjetasCredito
                .Include(t => t.Cliente)
                .AsQueryable();

            // Filtrar por cédula si se proporciona
            if (!string.IsNullOrEmpty(cedula))
            {
                query = query.Where(t => t.Cliente.Cedula == cedula);
            }

            // Filtrar por estado si se proporciona
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

        /// <summary>
        /// Genera un número de tarjeta único de 16 dígitos
        /// </summary>
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

        /// <summary>
        /// Verifica si un número de tarjeta ya existe
        /// </summary>
        public async Task<bool> ExisteNumeroTarjetaAsync(string numeroTarjeta)
        {
            return await _context.TarjetasCredito
                .AnyAsync(t => t.NumeroTarjeta == numeroTarjeta);
        }

        /// <summary>
        /// Cuenta cuántas tarjetas activas hay en total
        /// </summary>
        public async Task<int> ContarTarjetasActivasAsync()
        {
            return await _context.TarjetasCredito
                .CountAsync(t => t.EstaActiva);
        }
    }
}
