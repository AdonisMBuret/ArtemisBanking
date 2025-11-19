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

        public override async Task<CuentaAhorro> ObtenerPorIdAsync(int id)
        {
            return await _context.CuentasAhorro
                .Include(c => c.Usuario)
                .Include(c => c.Transacciones)
                .FirstOrDefaultAsync(c => c.Id == id);
        }


        public async Task<CuentaAhorro> ObtenerPorNumeroCuentaAsync(string numeroCuenta)
        {
            return await _context.CuentasAhorro
                .Include(c => c.Usuario)
                .Include(c => c.Transacciones)
                .FirstOrDefaultAsync(c => c.NumeroCuenta == numeroCuenta);
        }


        public async Task<CuentaAhorro> ObtenerCuentaPrincipalAsync(string usuarioId)
        {
            return await _context.CuentasAhorro
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId && c.EsPrincipal);
        }

        public async Task<IEnumerable<CuentaAhorro>> ObtenerCuentasDeUsuarioAsync(string usuarioId)
        {
            return await _context.CuentasAhorro
                .Include(c => c.Usuario)
                .Where(c => c.UsuarioId == usuarioId)
                .OrderByDescending(c => c.EsPrincipal) 
                .ThenByDescending(c => c.Balance) 
                .ToListAsync();
        }

        public async Task<IEnumerable<CuentaAhorro>> ObtenerCuentasActivasDeUsuarioAsync(string usuarioId)
        {
            return await _context.CuentasAhorro
                .Where(c => c.UsuarioId == usuarioId && c.EstaActiva)
                .OrderByDescending(c => c.EsPrincipal)
                .ThenByDescending(c => c.Balance)
                .ToListAsync();
        }

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

            if (!string.IsNullOrEmpty(cedula))
            {
                query = query.Where(c => c.Usuario.Cedula == cedula);
            }

            if (estaActiva.HasValue)
            {
                query = query.Where(c => c.EstaActiva == estaActiva.Value);
            }

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

        public async Task<bool> ExisteNumeroCuentaAsync(string numeroCuenta)
        {
            return await _context.CuentasAhorro
                .AnyAsync(c => c.NumeroCuenta == numeroCuenta);
        }
    }
}
