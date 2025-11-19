using ArtemisBanking.Domain.Entities;


namespace ArtemisBanking.Domain.Interfaces.Repositories
{
    public interface IRepositorioTarjetaCredito : IRepositorioGenerico<TarjetaCredito>
    {

        Task<TarjetaCredito> ObtenerPorNumeroTarjetaAsync(string numeroTarjeta);

        Task<IEnumerable<TarjetaCredito>> ObtenerTarjetasDeUsuarioAsync(string usuarioId);

        Task<IEnumerable<TarjetaCredito>> ObtenerTarjetasActivasDeUsuarioAsync(string usuarioId);

        Task<(IEnumerable<TarjetaCredito> tarjetas, int total)> ObtenerTarjetasPaginadasAsync(
            int pagina,
            int tamano,
            string cedula = null,
            bool? estaActiva = null);

        Task<string> GenerarNumeroTarjetaUnicoAsync();

        Task<bool> ExisteNumeroTarjetaAsync(string numeroTarjeta);

        Task<int> ContarTarjetasActivasAsync();
    }
}
