using ArtemisBanking.Application.Interfaces.Repositories;
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
    public class RepositorioCuotaPrestamo : RepositorioGenerico<CuotaPrestamo>, IRepositorioCuotaPrestamo
    {
        public RepositorioCuotaPrestamo(ArtemisBankingDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Obtiene todas las cuotas de un préstamo ordenadas por fecha
        /// </summary>
        public async Task<IEnumerable<CuotaPrestamo>> ObtenerCuotasDePrestamoAsync(int prestamoId)
        {
            return await _context.CuotasPrestamo
                .Where(c => c.PrestamoId == prestamoId)
                .OrderBy(c => c.FechaPago)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene la primera cuota que no ha sido pagada
        /// Se usa para aplicar pagos en orden secuencial
        /// </summary>
        public async Task<CuotaPrestamo> ObtenerPrimeraCuotaPendienteAsync(int prestamoId)
        {
            return await _context.CuotasPrestamo
                .Where(c => c.PrestamoId == prestamoId && !c.EstaPagada)
                .OrderBy(c => c.FechaPago)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Actualiza las cuotas atrasadas (ejecutado por Hangfire diariamente)
        /// Marca como atrasadas las cuotas cuya fecha de pago ya pasó y no están pagadas
        /// </summary>
        public async Task ActualizarCuotasAtrasadasAsync()
        {
            var hoy = DateTime.Now.Date;

            // Buscar cuotas vencidas y no pagadas
            var cuotasAtrasadas = await _context.CuotasPrestamo
                .Where(c => c.FechaPago < hoy && !c.EstaPagada && !c.EstaAtrasada)
                .ToListAsync();

            // Marcarlas como atrasadas
            foreach (var cuota in cuotasAtrasadas)
            {
                cuota.EstaAtrasada = true;
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Obtiene las cuotas futuras de un préstamo
        /// Se usa cuando se recalcula la tasa de interés
        /// </summary>
        public async Task<IEnumerable<CuotaPrestamo>> ObtenerCuotasFuturasAsync(int prestamoId)
        {
            var hoy = DateTime.Now.Date;

            return await _context.CuotasPrestamo
                .Where(c => c.PrestamoId == prestamoId && c.FechaPago > hoy && !c.EstaPagada)
                .OrderBy(c => c.FechaPago)
                .ToListAsync();
        }
    }
}
