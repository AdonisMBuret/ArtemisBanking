using ArtemisBanking.Application.Common;
using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ArtemisBanking.Domain.Interfaces.Repositories;

namespace ArtemisBanking.Infrastructure.Repositories
{
    public class RepositorioTransaccion : RepositorioGenerico<Transaccion>, IRepositorioTransaccion
    {
        public RepositorioTransaccion(ArtemisBankingDbContext context) : base(context)
        {
        }

        /// Obtiene todas las transacciones de una cuenta ordenadas por fecha

        public async Task<IEnumerable<Transaccion>> ObtenerTransaccionesDeCuentaAsync(int cuentaId)
        {
            return await _context.Transacciones
                .Where(t => t.CuentaAhorroId == cuentaId)
                .OrderByDescending(t => t.FechaTransaccion)
                .ToListAsync();
        }

         
        /// Cuenta el total de transacciones en el sistema
         
        public async Task<int> ContarTransaccionesTotalesAsync()
        {
            return await _context.Transacciones
                .Where(t => t.EstadoTransaccion == Constantes.EstadoAprobada)
                .CountAsync();
        }

         
        /// Cuenta las transacciones realizadas hoy
         
        public async Task<int> ContarTransaccionesDelDiaAsync()
        {
            var hoy = DateTime.Now.Date;
            return await _context.Transacciones
                .Where(t => t.FechaTransaccion.Date == hoy &&
                           t.EstadoTransaccion == Constantes.EstadoAprobada)
                .CountAsync();
        }

         
        /// Cuenta el total de pagos (a tarjetas y préstamos)
        /// Los pagos son transacciones donde el beneficiario es una tarjeta o préstamo
         
        public async Task<int> ContarPagosTotalesAsync()
        {
            return await _context.Transacciones
                .Where(t => t.EstadoTransaccion == Constantes.EstadoAprobada &&
                           t.TipoTransaccion == Constantes.TipoDebito &&
                           (t.Beneficiario.Length == 16 || t.Beneficiario.Length == 9))
                .CountAsync();
        }

         
        /// Cuenta los pagos realizados hoy
         
        public async Task<int> ContarPagosDelDiaAsync()
        {
            var hoy = DateTime.Now.Date;
            return await _context.Transacciones
                .Where(t => t.FechaTransaccion.Date == hoy &&
                           t.EstadoTransaccion == Constantes.EstadoAprobada &&
                           t.TipoTransaccion == Constantes.TipoDebito &&
                           (t.Beneficiario.Length == 16 || t.Beneficiario.Length == 9))
                .CountAsync();
        }



        public async Task<int> ContarDepositosDelDiaAsync()
        {
            var hoy = DateTime.Now.Date;
            return await _context.Transacciones
                .Where(t => t.FechaTransaccion.Date == hoy &&
                           t.EstadoTransaccion == Constantes.EstadoAprobada &&
                           t.TipoTransaccion == Constantes.TipoCredito &&
                           t.Origen == Constantes.TextoDeposito)
                .CountAsync();
        }

         
        /// Cuenta los retiros realizados hoy por cajero
         
        public async Task<int> ContarRetirosDelDiaAsync()
        {
            var hoy = DateTime.Now.Date;
            return await _context.Transacciones
                .Where(t => t.FechaTransaccion.Date == hoy &&
                           t.EstadoTransaccion == Constantes.EstadoAprobada &&
                           t.TipoTransaccion == Constantes.TipoDebito &&
                           t.Beneficiario == Constantes.TextoRetiro)
                .CountAsync();
        }

        /// <summary>
        /// Obtiene transacciones paginadas de una cuenta específica
        /// </summary>
        public async Task<(IEnumerable<Transaccion> Transacciones, int TotalRegistros)> ObtenerPorCuentaPaginadoAsync(
            int cuentaId, 
            int pagina, 
            int registrosPorPagina)
        {
            var query = _context.Transacciones
                .Where(t => t.CuentaAhorroId == cuentaId)
                .OrderByDescending(t => t.FechaTransaccion);

            var totalRegistros = await query.CountAsync();

            var transacciones = await query
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();

            return (transacciones, totalRegistros);
        }
    }
}

