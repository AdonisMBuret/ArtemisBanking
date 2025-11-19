using ArtemisBanking.Domain.Entities;

namespace ArtemisBanking.Domain.Interfaces.Repositories
{
    public interface IRepositorioPrestamo : IRepositorioGenerico<Prestamo>
    {
        Task<Prestamo> ObtenerPorNumeroPrestamoAsync(string numeroPrestamo);

        Task<Prestamo> ObtenerPrestamoActivoDeUsuarioAsync(string usuarioId);

        Task<IEnumerable<Prestamo>> ObtenerPrestamosDeUsuarioAsync(string usuarioId);

        Task<(IEnumerable<Prestamo> prestamos, int total)> ObtenerPrestamosPaginadosAsync(
            int pagina,
            int tamano,
            string cedula = null,
            bool? estaActivo = null);

        Task<decimal> CalcularDeudaPromedioAsync();

        Task<decimal> CalcularDeudaTotalClienteAsync(string usuarioId);

        Task<string> GenerarNumeroPrestamoUnicoAsync();

        Task<bool> TienePrestamoActivoAsync(string usuarioId);

        Task<int> ContarPrestamosActivosAsync();
    }
}
