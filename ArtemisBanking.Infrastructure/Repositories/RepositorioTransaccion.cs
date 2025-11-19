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

        public async Task<IEnumerable<Transaccion>> ObtenerTransaccionesDeCuentaAsync(int cuentaId)
        {
            return await _context.Transacciones
                .Where(t => t.CuentaAhorroId == cuentaId)
                .OrderByDescending(t => t.FechaTransaccion)
                .ToListAsync();
        }

        public async Task<int> ContarTransaccionesTotalesAsync()
        {
            return await _context.Transacciones
                .Where(t => t.EstadoTransaccion == Constantes.EstadoAprobada)
                .CountAsync();
        }

        public async Task<int> ContarTransaccionesDelDiaAsync()
        {
            var hoy = DateTime.Now.Date;
            return await _context.Transacciones
                .Where(t => t.FechaTransaccion.Date == hoy &&
                           t.EstadoTransaccion == Constantes.EstadoAprobada)
                .CountAsync();
        }
        public async Task<int> ContarPagosTotalesAsync()
        {
            return await _context.Transacciones
                .Where(t => t.EstadoTransaccion == Constantes.EstadoAprobada &&
                           t.TipoTransaccion == Constantes.TipoDebito &&
                           (t.Beneficiario.Length == 16 || t.Beneficiario.Length == 9))
                .CountAsync();
        }

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

