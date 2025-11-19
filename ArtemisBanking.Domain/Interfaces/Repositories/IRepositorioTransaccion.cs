using ArtemisBanking.Domain.Entities;

namespace ArtemisBanking.Domain.Interfaces.Repositories
{
    public interface IRepositorioTransaccion : IRepositorioGenerico<Transaccion>
    {
        Task<IEnumerable<Transaccion>> ObtenerTransaccionesDeCuentaAsync(int cuentaId);

        Task<(IEnumerable<Transaccion> Transacciones, int TotalRegistros)> ObtenerPorCuentaPaginadoAsync(
            int cuentaId, 
            int pagina, 
            int registrosPorPagina);

        Task<int> ContarTransaccionesTotalesAsync();

        Task<int> ContarTransaccionesDelDiaAsync();

        Task<int> ContarPagosTotalesAsync();

        Task<int> ContarPagosDelDiaAsync();
        
        Task<int> ContarDepositosDelDiaAsync();

        Task<int> ContarRetirosDelDiaAsync();
    }
}
