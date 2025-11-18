using ArtemisBanking.Domain.Entities;

namespace ArtemisBanking.Domain.Interfaces.Repositories
{
    public interface IRepositorioTransaccion : IRepositorioGenerico<Transaccion>
    {
        // Obtener transacciones de una cuenta
        Task<IEnumerable<Transaccion>> ObtenerTransaccionesDeCuentaAsync(int cuentaId);

        // Obtener transacciones paginadas de una cuenta
        Task<(IEnumerable<Transaccion> Transacciones, int TotalRegistros)> ObtenerPorCuentaPaginadoAsync(
            int cuentaId, 
            int pagina, 
            int registrosPorPagina);

        // Contar transacciones totales
        Task<int> ContarTransaccionesTotalesAsync();

        // Contar transacciones del día
        Task<int> ContarTransaccionesDelDiaAsync();

        // Contar pagos totales (a tarjetas y préstamos)
        Task<int> ContarPagosTotalesAsync();

        // Contar pagos del día
        Task<int> ContarPagosDelDiaAsync();
        
        Task<int> ContarDepositosDelDiaAsync();

        Task<int> ContarRetirosDelDiaAsync();
    }
}
