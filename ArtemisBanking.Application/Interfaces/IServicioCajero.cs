using ArtemisBanking.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Application.Interfaces
{
    public interface IServicioCajero
    {
        /// <summary>
        /// Realiza un depósito a una cuenta de ahorro
        /// Acredita el monto y registra la transacción
        /// Envía correo de notificación al cliente
        /// </summary>
        Task<ResultadoOperacion> RealizarDepositoAsync(DepositoCajeroDTO datos);

        /// <summary>
        /// Realiza un retiro de una cuenta de ahorro
        /// Valida fondos suficientes, debita y registra la transacción
        /// Envía correo de notificación al cliente
        /// </summary>
        Task<ResultadoOperacion> RealizarRetiroAsync(RetiroCajeroDTO datos);

        /// <summary>
        /// Procesa un pago a tarjeta de crédito desde una cuenta
        /// Solo se paga hasta el monto de la deuda actual
        /// Envía correo de notificación al cliente
        /// </summary>
        Task<ResultadoOperacion> PagarTarjetaCreditoAsync(PagoTarjetaCajeroDTO datos);

        /// <summary>
        /// Procesa un pago a un préstamo desde una cuenta
        /// Aplica el pago a las cuotas pendientes de forma secuencial
        /// Si sobra dinero, lo devuelve a la cuenta
        /// Envía correo de notificación al cliente
        /// </summary>
        Task<ResultadoOperacion> PagarPrestamoAsync(PagoPrestamoCajeroDTO datos);

        /// <summary>
        /// Realiza una transacción entre dos cuentas de terceros
        /// Valida ambas cuentas, fondos suficientes y procesa la transferencia
        /// Envía correos a ambos clientes
        /// </summary>
        Task<ResultadoOperacion> TransaccionEntreTercerosAsync(TransaccionTercerosCajeroDTO datos);

        /// <summary>
        /// Obtiene el dashboard con las estadísticas del día del cajero
        /// </summary>
        Task<ResultadoOperacion<DashboardCajeroDTO>> ObtenerDashboardAsync(string cajeroId);
    }
}
