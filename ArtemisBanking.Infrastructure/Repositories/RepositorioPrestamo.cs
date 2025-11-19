using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ArtemisBanking.Domain.Interfaces.Repositories;

namespace ArtemisBanking.Infrastructure.Repositories
{
    public class RepositorioPrestamo : RepositorioGenerico<Prestamo>, IRepositorioPrestamo
    {
        public RepositorioPrestamo(ArtemisBankingDbContext context) : base(context)
        {
        }
        public override async Task<Prestamo> ObtenerPorIdAsync(int id)
        {
            return await _context.Prestamos
                .Include(p => p.Cliente)
                .Include(p => p.Administrador)
                .Include(p => p.TablaAmortizacion)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
         
        public async Task<Prestamo> ObtenerPorNumeroPrestamoAsync(string numeroPrestamo)
        {
            return await _context.Prestamos
                .Include(p => p.Cliente)
                .Include(p => p.Administrador)
                .Include(p => p.TablaAmortizacion)
                .FirstOrDefaultAsync(p => p.NumeroPrestamo == numeroPrestamo);
        }

         
        public async Task<Prestamo> ObtenerPrestamoActivoDeUsuarioAsync(string usuarioId)
        {
            return await _context.Prestamos
                .Include(p => p.TablaAmortizacion)
                .FirstOrDefaultAsync(p => p.ClienteId == usuarioId && p.EstaActivo);
        }

                
        public async Task<IEnumerable<Prestamo>> ObtenerPrestamosDeUsuarioAsync(string usuarioId)
        {
            return await _context.Prestamos
                .Include(p => p.TablaAmortizacion)
                .Where(p => p.ClienteId == usuarioId)
                .OrderByDescending(p => p.EstaActivo) 
                .ThenByDescending(p => p.FechaCreacion) 
                .ToListAsync();
        }

         
        public async Task<(IEnumerable<Prestamo> prestamos, int total)> ObtenerPrestamosPaginadosAsync(
            int pagina,
            int tamano,
            string cedula = null,
            bool? estaActivo = null)
        {
            var query = _context.Prestamos
                .Include(p => p.Cliente)
                .Include(p => p.TablaAmortizacion)
                .AsQueryable();

            if (!string.IsNullOrEmpty(cedula))
            {
                query = query.Where(p => p.Cliente.Cedula == cedula);
            }

            if (estaActivo.HasValue)
            {
                query = query.Where(p => p.EstaActivo == estaActivo.Value);
            }

            var total = await query.CountAsync();

            var prestamos = await query
                .OrderByDescending(p => p.FechaCreacion)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();

            return (prestamos, total);
        }

                 
        public async Task<decimal> CalcularDeudaPromedioAsync()
        {
            var clientesConProductos = await _context.Users
                .Include(u => u.Prestamos)
                .Include(u => u.TarjetasCredito)
                .Where(u => u.Prestamos.Any() || u.TarjetasCredito.Any())
                .ToListAsync();

            if (!clientesConProductos.Any())
                return 0;

            decimal deudaTotal = 0;

            foreach (var cliente in clientesConProductos)
            {
                var deudaPrestamos = cliente.Prestamos
                    .Where(p => p.EstaActivo)
                    .Sum(p => p.TablaAmortizacion.Where(c => !c.EstaPagada).Sum(c => c.MontoCuota));

                var deudaTarjetas = cliente.TarjetasCredito
                    .Where(t => t.EstaActiva)
                    .Sum(t => t.DeudaActual);

                deudaTotal += deudaPrestamos + deudaTarjetas;
            }

            return deudaTotal / clientesConProductos.Count;
        }
         
        public async Task<decimal> CalcularDeudaTotalClienteAsync(string usuarioId)
        {
            var deudaPrestamos = await _context.Prestamos
                .Where(p => p.ClienteId == usuarioId && p.EstaActivo)
                .SelectMany(p => p.TablaAmortizacion)
                .Where(c => !c.EstaPagada)
                .SumAsync(c => c.MontoCuota);

            var deudaTarjetas = await _context.TarjetasCredito
                .Where(t => t.ClienteId == usuarioId && t.EstaActiva)
                .SumAsync(t => t.DeudaActual);

            return deudaPrestamos + deudaTarjetas;
        }
        public async Task<string> GenerarNumeroPrestamoUnicoAsync()
        {
            string numeroPrestamo;
            var random = new Random();

            do
            {
                numeroPrestamo = "";
                for (int i = 0; i < 9; i++)
                {
                    numeroPrestamo += random.Next(0, 10).ToString();
                }
            }
            while (await _context.Prestamos.AnyAsync(p => p.NumeroPrestamo == numeroPrestamo) ||
                   await _context.CuentasAhorro.AnyAsync(c => c.NumeroCuenta == numeroPrestamo));

            return numeroPrestamo;
        }

        public async Task<bool> TienePrestamoActivoAsync(string usuarioId)
        {
            return await _context.Prestamos
                .AnyAsync(p => p.ClienteId == usuarioId && p.EstaActivo);
        }

        public async Task<int> ContarPrestamosActivosAsync()
        {
            return await _context.Prestamos
                .CountAsync(p => p.EstaActivo);
        }
    }
}
