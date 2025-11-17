using ArtemisBanking.Domain.Entities;


namespace ArtemisBanking.Domain.Interfaces.Repositories
{
    public interface IRepositorioTarjetaCredito : IRepositorioGenerico<TarjetaCredito>
    {
        // Obtener tarjeta por número
        Task<TarjetaCredito> ObtenerPorNumeroTarjetaAsync(string numeroTarjeta);

        // Obtener tarjetas de un usuario
        Task<IEnumerable<TarjetaCredito>> ObtenerTarjetasDeUsuarioAsync(string usuarioId);

        // Obtener tarjetas activas de un usuario
        Task<IEnumerable<TarjetaCredito>> ObtenerTarjetasActivasDeUsuarioAsync(string usuarioId);

        // Obtener tarjetas paginadas con filtros
        Task<(IEnumerable<TarjetaCredito> tarjetas, int total)> ObtenerTarjetasPaginadasAsync(
            int pagina,
            int tamano,
            string cedula = null,
            bool? estaActiva = null);

        // Generar número de tarjeta único
        Task<string> GenerarNumeroTarjetaUnicoAsync();

        // Verificar si un número de tarjeta ya existe
        Task<bool> ExisteNumeroTarjetaAsync(string numeroTarjeta);

        // Contar tarjetas activas
        Task<int> ContarTarjetasActivasAsync();
    }
}
