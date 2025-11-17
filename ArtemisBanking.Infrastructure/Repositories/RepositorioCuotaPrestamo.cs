using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ArtemisBanking.Domain.Interfaces.Repositories;

namespace ArtemisBanking.Infrastructure.Repositories
{
    public class RepositorioCuotaPrestamo : RepositorioGenerico<CuotaPrestamo>, IRepositorioCuotaPrestamo
    {
        public RepositorioCuotaPrestamo(ArtemisBankingDbContext context) : base(context)
        {
        }

        /// Obtiene todas las cuotas de un préstamo ordenadas por fecha
        public async Task<IEnumerable<CuotaPrestamo>> ObtenerCuotasDePrestamoAsync(int prestamoId)
        {
            return await _context.CuotasPrestamo
                .Where(c => c.PrestamoId == prestamoId)
                .OrderBy(c => c.FechaPago)
                .ToListAsync();
        }

        /// Obtiene la primera cuota que no ha sido pagada
        /// Se usa para aplicar pagos en orden secuencial
        public async Task<CuotaPrestamo> ObtenerPrimeraCuotaPendienteAsync(int prestamoId)
        {
            return await _context.CuotasPrestamo
                .Where(c => c.PrestamoId == prestamoId && !c.EstaPagada)
                .OrderBy(c => c.FechaPago)
                .FirstOrDefaultAsync();
        }

        /// Actualiza las cuotas atrasadas (ejecutado por Hangfire diariamente)
        /// Marca como atrasadas las cuotas cuya fecha de pago ya pasó y no están pagadas
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

        /// Obtiene las cuotas futuras de un préstamo
        /// Se usa cuando se recalcula la tasa de interés
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
