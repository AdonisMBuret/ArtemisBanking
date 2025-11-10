using ArtemisBanking.Application.Common;
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
    public class RepositorioTransaccion : RepositorioGenerico<Transaccion>, IRepositorioTransaccion
    {
        public RepositorioTransaccion(ArtemisBankingDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Obtiene todas las transacciones de una cuenta ordenadas por fecha
        /// </summary>
        public async Task<IEnumerable<Transaccion>> ObtenerTransaccionesDeCuentaAsync(int cuentaId)
        {
            return await _context.Transacciones
                .Where(t => t.CuentaAhorroId == cuentaId)
                .OrderByDescending(t => t.FechaTransaccion)
                .ToListAsync();
        }

        /// <summary>
        /// Cuenta el total de transacciones en el sistema
        /// </summary>
        public async Task<int> ContarTransaccionesTotalesAsync()
        {
            return await _context.Transacciones
                .Where(t => t.EstadoTransaccion == Constantes.EstadoAprobada)
                .CountAsync();
        }

        /// <summary>
        /// Cuenta las transacciones realizadas hoy
        /// </summary>
        public async Task<int> ContarTransaccionesDelDiaAsync()
        {
            var hoy = DateTime.Now.Date;
            return await _context.Transacciones
                .Where(t => t.FechaTransaccion.Date == hoy &&
                           t.EstadoTransaccion == Constantes.EstadoAprobada)
                .CountAsync();
        }

        /// <summary>
        /// Cuenta el total de pagos (a tarjetas y préstamos)
        /// Los pagos son transacciones donde el beneficiario es una tarjeta o préstamo
        /// </summary>
        public async Task<int> ContarPagosTotalesAsync()
        {
            return await _context.Transacciones
                .Where(t => t.EstadoTransaccion == Constantes.EstadoAprobada &&
                           t.TipoTransaccion == Constantes.TipoDebito &&
                           (t.Beneficiario.Length == 16 || t.Beneficiario.Length == 9))
                .CountAsync();
        }

        /// <summary>
        /// Cuenta los pagos realizados hoy
        /// </summary>
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
    }
}

