using ArtemisBanking.Application.DTOs;

namespace ArtemisBanking.Application.Interfaces
{
    public interface IServicioCajero
    {
         
        /// Realiza un depósito a una cuenta de ahorro
        /// Acredita el monto y registra la transacción
        /// Envía correo de notificación al cliente
         
        Task<ResultadoOperacion> RealizarDepositoAsync(DepositoCajeroDTO datos);

         
        /// Realiza un retiro de una cuenta de ahorro
        /// Valida fondos suficientes, debita y registra la transacción
        /// Envía correo de notificación al cliente
         
        Task<ResultadoOperacion> RealizarRetiroAsync(RetiroCajeroDTO datos);

         
        /// Procesa un pago a tarjeta de crédito desde una cuenta
        /// Solo se paga hasta el monto de la deuda actual
        /// Envía correo de notificación al cliente
         
        Task<ResultadoOperacion> PagarTarjetaCreditoAsync(PagoTarjetaCajeroDTO datos);

         
        /// Procesa un pago a un préstamo desde una cuenta
        /// Aplica el pago a las cuotas pendientes de forma secuencial
        /// Si sobra dinero, lo devuelve a la cuenta
        /// Envía correo de notificación al cliente
         
        Task<ResultadoOperacion> PagarPrestamoAsync(PagoPrestamoCajeroDTO datos);

         
        /// Realiza una transacción entre dos cuentas de terceros
        /// Valida ambas cuentas, fondos suficientes y procesa la transferencia
        /// Envía correos a ambos clientes
         
        Task<ResultadoOperacion> TransaccionEntreTercerosAsync(TransaccionTercerosCajeroDTO datos);

         
        /// Obtiene el dashboard con las estadísticas del día del cajero
         
        Task<ResultadoOperacion<DashboardCajeroDTO>> ObtenerDashboardAsync(string cajeroId);
    }
}
