using ArtemisBanking.Domain.Entities;


namespace ArtemisBanking.Domain.Interfaces.Repositories
{
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
