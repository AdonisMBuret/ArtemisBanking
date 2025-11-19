using ArtemisBanking.Domain.Entities;


namespace ArtemisBanking.Domain.Interfaces.Repositories
{
    public interface IRepositorioCuotaPrestamo : IRepositorioGenerico<CuotaPrestamo>
    {

        Task<IEnumerable<CuotaPrestamo>> ObtenerCuotasDePrestamoAsync(int prestamoId);

        Task<CuotaPrestamo> ObtenerPrimeraCuotaPendienteAsync(int prestamoId);

        Task ActualizarCuotasAtrasadasAsync();

        Task<IEnumerable<CuotaPrestamo>> ObtenerCuotasFuturasAsync(int prestamoId);
    }
}
