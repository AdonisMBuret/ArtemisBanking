using ArtemisBanking.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtemisBanking.Application.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz para operaciones con transacciones
    /// </summary>
    public interface IRepositorioTransaccion : IRepositorioGenerico<Transaccion>
    {
        // Obtener transacciones de una cuenta
        Task<IEnumerable<Transaccion>> ObtenerTransaccionesDeCuentaAsync(int cuentaId);
        
        // Contar transacciones totales
        Task<int> ContarTransaccionesTotalesAsync();
        
        // Contar transacciones del día
        Task<int> ContarTransaccionesDelDiaAsync();
        
        // Contar pagos totales (a tarjetas y préstamos)
        Task<int> ContarPagosTotalesAsync();
        
        // Contar pagos del día
        Task<int> ContarPagosDelDiaAsync();
    }
}