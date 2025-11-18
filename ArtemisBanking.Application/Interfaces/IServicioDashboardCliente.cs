using ArtemisBanking.Application.DTOs;

namespace ArtemisBanking.Application.Interfaces
{
    /// <summary>
    /// Servicio para Dashboard del Cliente
    /// Centraliza la lógica de negocio para obtener todos los productos del cliente
    /// </summary>
    public interface IServicioDashboardCliente
    {
        /// <summary>
        /// Obtiene todos los productos financieros del cliente
        /// (Cuentas, Préstamos, Tarjetas)
        /// </summary>
        Task<ResultadoOperacion<DashboardClienteDTO>> ObtenerDashboardAsync(string usuarioId);

        /// <summary>
        /// Obtiene el detalle de una cuenta con sus transacciones
        /// </summary>
        Task<ResultadoOperacion<DetalleCuentaClienteDTO>> ObtenerDetalleCuentaAsync(int cuentaId, string usuarioId);

        /// <summary>
        /// Obtiene el detalle de un préstamo con su tabla de amortización
        /// </summary>
        Task<ResultadoOperacion<DetallePrestamoClienteDTO>> ObtenerDetallePrestamoAsync(int prestamoId, string usuarioId);

        /// <summary>
        /// Obtiene el detalle de una tarjeta con sus consumos
        /// </summary>
        Task<ResultadoOperacion<DetalleTarjetaClienteDTO>> ObtenerDetalleTarjetaAsync(int tarjetaId, string usuarioId);
    }
}
