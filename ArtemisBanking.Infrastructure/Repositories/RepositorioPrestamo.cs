using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtemisBanking.Domain.Interfaces.Repositories;

namespace ArtemisBanking.Infrastructure.Repositories
{
    public class RepositorioPrestamo : RepositorioGenerico<Prestamo>, IRepositorioPrestamo
    {
        public RepositorioPrestamo(ArtemisBankingDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Obtiene un préstamo por su número, incluyendo toda su información
        /// </summary>
        public async Task<Prestamo> ObtenerPorNumeroPrestamoAsync(string numeroPrestamo)
        {
            return await _context.Prestamos
                .Include(p => p.Cliente)
                .Include(p => p.Administrador)
                .Include(p => p.TablaAmortizacion)
                .FirstOrDefaultAsync(p => p.NumeroPrestamo == numeroPrestamo);
        }

        /// <summary>
        /// Obtiene el préstamo activo de un usuario
        /// Un cliente solo puede tener un préstamo activo a la vez
        /// </summary>
        public async Task<Prestamo> ObtenerPrestamoActivoDeUsuarioAsync(string usuarioId)
        {
            return await _context.Prestamos
                .Include(p => p.TablaAmortizacion)
                .FirstOrDefaultAsync(p => p.ClienteId == usuarioId && p.EstaActivo);
        }

        /// <summary>
        /// Obtiene todos los préstamos de un usuario (activos y completados)
        /// </summary>
        public async Task<IEnumerable<Prestamo>> ObtenerPrestamosDeUsuarioAsync(string usuarioId)
        {
            return await _context.Prestamos
                .Include(p => p.TablaAmortizacion)
                .Where(p => p.ClienteId == usuarioId)
                .OrderByDescending(p => p.EstaActivo) // Activos primero
                .ThenByDescending(p => p.FechaCreacion) // Más recientes primero
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene préstamos paginados con filtros opcionales
        /// </summary>
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

            // Filtrar por cédula si se proporciona
            if (!string.IsNullOrEmpty(cedula))
            {
                query = query.Where(p => p.Cliente.Cedula == cedula);
            }

            // Filtrar por estado si se proporciona
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

        /// <summary>
        /// Calcula la deuda promedio de todos los clientes
        /// Suma préstamos pendientes + deudas de tarjetas / número de clientes
        /// </summary>
        public async Task<decimal> CalcularDeudaPromedioAsync()
        {
            // Obtener todos los clientes que tienen productos financieros
            var clientesConProductos = await _context.Users
                .Include(u => u.Prestamos)
                .Include(u => u.TarjetasCredito)
                .Where(u => u.Prestamos.Any() || u.TarjetasCredito.Any())
                .ToListAsync();

            if (!clientesConProductos.Any())
                return 0;

            // Calcular deuda total de cada cliente
            decimal deudaTotal = 0;

            foreach (var cliente in clientesConProductos)
            {
                // Sumar deuda de préstamos activos
                var deudaPrestamos = cliente.Prestamos
                    .Where(p => p.EstaActivo)
                    .Sum(p => p.TablaAmortizacion.Where(c => !c.EstaPagada).Sum(c => c.MontoCuota));

                // Sumar deuda de tarjetas activas
                var deudaTarjetas = cliente.TarjetasCredito
                    .Where(t => t.EstaActiva)
                    .Sum(t => t.DeudaActual);

                deudaTotal += deudaPrestamos + deudaTarjetas;
            }

            return deudaTotal / clientesConProductos.Count;
        }

        /// <summary>
        /// Calcula la deuda total de un cliente específico
        /// Incluye préstamos activos y tarjetas de crédito
        /// </summary>
        public async Task<decimal> CalcularDeudaTotalClienteAsync(string usuarioId)
        {
            // Deuda de préstamos activos (cuotas pendientes)
            var deudaPrestamos = await _context.Prestamos
                .Where(p => p.ClienteId == usuarioId && p.EstaActivo)
                .SelectMany(p => p.TablaAmortizacion)
                .Where(c => !c.EstaPagada)
                .SumAsync(c => c.MontoCuota);

            // Deuda de tarjetas activas
            var deudaTarjetas = await _context.TarjetasCredito
                .Where(t => t.ClienteId == usuarioId && t.EstaActiva)
                .SumAsync(t => t.DeudaActual);

            return deudaPrestamos + deudaTarjetas;
        }

        /// <summary>
        /// Genera un número de préstamo único de 9 dígitos
        /// Verifica que no exista ni en préstamos ni en cuentas
        /// </summary>
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

        /// <summary>
        /// Verifica si un cliente tiene un préstamo activo
        /// </summary>
        public async Task<bool> TienePrestamoActivoAsync(string usuarioId)
        {
            return await _context.Prestamos
                .AnyAsync(p => p.ClienteId == usuarioId && p.EstaActivo);
        }

        /// <summary>
        /// Cuenta cuántos préstamos activos hay en total
        /// </summary>
        public async Task<int> ContarPrestamosActivosAsync()
        {
            return await _context.Prestamos
                .CountAsync(p => p.EstaActivo);
        }
    }
}
