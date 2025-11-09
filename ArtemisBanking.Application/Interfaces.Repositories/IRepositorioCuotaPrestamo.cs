using ArtemisBanking.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtemisBanking.Application.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz para operaciones con cuotas de préstamos
    /// </summary>
    public interface IRepositorioCuotaPrestamo : IRepositorioGenerico<CuotaPrestamo>
    {
        // Obtener todas las cuotas de un préstamo
        Task<IEnumerable<CuotaPrestamo>> ObtenerCuotasDePrestamoAsync(int prestamoId);
        
        // Obtener primera cuota pendiente de un préstamo
        Task<CuotaPrestamo> ObtenerPrimeraCuotaPendienteAsync(int prestamoId);
        
        // Actualizar cuotas atrasadas (job de Hangfire)
        Task ActualizarCuotasAtrasadasAsync();
        
        // Obtener cuotas futuras (para recalcular cuando cambia la tasa)
        Task<IEnumerable<CuotaPrestamo>> ObtenerCuotasFuturasAsync(int prestamoId);
    }
}