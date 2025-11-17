using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ArtemisBanking.Domain.Interfaces.Repositories;

namespace ArtemisBanking.Infrastructure.Repositories
{
    public class RepositorioCuentaAhorro : RepositorioGenerico<CuentaAhorro>, IRepositorioCuentaAhorro
    {
        public RepositorioCuentaAhorro(ArtemisBankingDbContext context) : base(context)
        {
        }

        /// Busca una cuenta por su número único
        /// Incluye las relaciones con usuario y transacciones
        public async Task<CuentaAhorro> ObtenerPorNumeroCuentaAsync(string numeroCuenta)
        {
            return await _context.CuentasAhorro
                .Include(c => c.Usuario)
                .Include(c => c.Transacciones)
                .FirstOrDefaultAsync(c => c.NumeroCuenta == numeroCuenta);
        }

        /// Obtiene la cuenta principal de un usuario
        /// Cada cliente tiene una cuenta principal
        public async Task<CuentaAhorro> ObtenerCuentaPrincipalAsync(string usuarioId)
        {
            return await _context.CuentasAhorro
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId && c.EsPrincipal);
        }

        /// Obtiene todas las cuentas (principal y secundarias) de un usuario
        public async Task<IEnumerable<CuentaAhorro>> ObtenerCuentasDeUsuarioAsync(string usuarioId)
        {
            return await _context.CuentasAhorro
                .Include(c => c.Usuario)
                .Where(c => c.UsuarioId == usuarioId)
                .OrderByDescending(c => c.EsPrincipal) // Principal primero
                .ThenByDescending(c => c.Balance) // Luego por balance
                .ToListAsync();
        }

        /// Obtiene solo las cuentas activas de un usuario
        public async Task<IEnumerable<CuentaAhorro>> ObtenerCuentasActivasDeUsuarioAsync(string usuarioId)
        {
            return await _context.CuentasAhorro
                .Where(c => c.UsuarioId == usuarioId && c.EstaActiva)
                .OrderByDescending(c => c.EsPrincipal)
                .ThenByDescending(c => c.Balance)
                .ToListAsync();
        }

        /// Obtiene cuentas paginadas con filtros opcionales
        public async Task<(IEnumerable<CuentaAhorro> cuentas, int total)> ObtenerCuentasPaginadasAsync(
            int pagina,
            int tamano,
            string cedula = null,
            bool? estaActiva = null,
            bool? esPrincipal = null)
        {
            var query = _context.CuentasAhorro
                .Include(c => c.Usuario)
                .AsQueryable();

            // Filtrar por cédula si se proporciona
            if (!string.IsNullOrEmpty(cedula))
            {
                query = query.Where(c => c.Usuario.Cedula == cedula);
            }

            // Filtrar por estado si se proporciona
            if (estaActiva.HasValue)
            {
                query = query.Where(c => c.EstaActiva == estaActiva.Value);
            }

            // Filtrar por tipo (principal/secundaria) si se proporciona
            if (esPrincipal.HasValue)
            {
                query = query.Where(c => c.EsPrincipal == esPrincipal.Value);
            }

            var total = await query.CountAsync();

            var cuentas = await query
                .OrderByDescending(c => c.FechaCreacion)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();

            return (cuentas, total);
        }

        /// Genera un número de cuenta único de 9 dígitos
        /// Verifica que no exista ni en cuentas ni en préstamos
        public async Task<string> GenerarNumeroCuentaUnicoAsync()
        {
            string numeroCuenta;
            var random = new Random();

            do
            {
                numeroCuenta = "";
                for (int i = 0; i < 9; i++)
                {
                    numeroCuenta += random.Next(0, 10).ToString();
                }
            }
            while (await ExisteNumeroCuentaAsync(numeroCuenta) ||
                   await _context.Prestamos.AnyAsync(p => p.NumeroPrestamo == numeroCuenta));

            return numeroCuenta;
        }

        /// Verifica si un número de cuenta ya existe
        public async Task<bool> ExisteNumeroCuentaAsync(string numeroCuenta)
        {
            return await _context.CuentasAhorro
                .AnyAsync(c => c.NumeroCuenta == numeroCuenta);
        }
    }
}
