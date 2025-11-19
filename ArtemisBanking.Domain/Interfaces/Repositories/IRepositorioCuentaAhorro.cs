using ArtemisBanking.Domain.Entities;


namespace ArtemisBanking.Domain.Interfaces.Repositories
{
    public interface IRepositorioCuentaAhorro : IRepositorioGenerico<CuentaAhorro>
    {
        Task<CuentaAhorro> ObtenerPorNumeroCuentaAsync(string numeroCuenta);

        Task<CuentaAhorro> ObtenerCuentaPrincipalAsync(string usuarioId);

        Task<IEnumerable<CuentaAhorro>> ObtenerCuentasDeUsuarioAsync(string usuarioId);

        Task<IEnumerable<CuentaAhorro>> ObtenerCuentasActivasDeUsuarioAsync(string usuarioId);

        Task<(IEnumerable<CuentaAhorro> cuentas, int total)> ObtenerCuentasPaginadasAsync(
            int pagina,
            int tamano,
            string cedula = null,
            bool? estaActiva = null,
            bool? esPrincipal = null);

        Task<string> GenerarNumeroCuentaUnicoAsync();

        Task<bool> ExisteNumeroCuentaAsync(string numeroCuenta);
    }
}
