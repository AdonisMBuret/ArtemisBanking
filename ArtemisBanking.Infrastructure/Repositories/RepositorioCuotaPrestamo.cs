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

        public async Task<IEnumerable<CuotaPrestamo>> ObtenerCuotasDePrestamoAsync(int prestamoId)
        {
            return await _context.CuotasPrestamo
                .Where(c => c.PrestamoId == prestamoId)
                .OrderBy(c => c.FechaPago)
                .ToListAsync();
        }

        public async Task<CuotaPrestamo> ObtenerPrimeraCuotaPendienteAsync(int prestamoId)
        {
            return await _context.CuotasPrestamo
                .Where(c => c.PrestamoId == prestamoId && !c.EstaPagada)
                .OrderBy(c => c.FechaPago)
                .FirstOrDefaultAsync();
        }

        public async Task ActualizarCuotasAtrasadasAsync()
        {
            var hoy = DateTime.Now.Date;

            var cuotasAtrasadas = await _context.CuotasPrestamo
                .Where(c => c.FechaPago < hoy && !c.EstaPagada && !c.EstaAtrasada)
                .ToListAsync();

            foreach (var cuota in cuotasAtrasadas)
            {
                cuota.EstaAtrasada = true;
            }

            await _context.SaveChangesAsync();
        }

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
