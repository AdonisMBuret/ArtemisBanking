using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Application.Interfaces
{
    public interface IServicioTransaccion
    {
        /// <summary>
        /// Realiza una transferencia entre dos cuentas de ahorro
        /// Valida fondos, actualiza balances y registra transacciones
        /// </summary>
        Task<(bool exito, string mensaje)> RealizarTransferenciaAsync(
            int cuentaOrigenId,
            string numeroCuentaDestino,
            decimal monto);

        /// <summary>
        /// Procesa un pago a una tarjeta de crédito desde una cuenta de ahorro
        /// Solo se paga hasta el monto de la deuda actual
        /// </summary>
        Task<(bool exito, string mensaje, decimal montoPagado)> PagarTarjetaCreditoAsync(
            int cuentaOrigenId,
            int tarjetaId,
            decimal monto);

        /// <summary>
        /// Procesa un pago a un préstamo desde una cuenta de ahorro
        /// Aplica el pago a las cuotas pendientes de forma secuencial
        /// </summary>
        Task<(bool exito, string mensaje, decimal montoAplicado)> PagarPrestamoAsync(
            int cuentaOrigenId,
            int prestamoId,
            decimal monto);

        /// <summary>
        /// Realiza un avance de efectivo desde una tarjeta a una cuenta
        /// Aplica un interés del 6.25% sobre el monto
        /// </summary>
        Task<(bool exito, string mensaje)> RealizarAvanceEfectivoAsync(
            int tarjetaId,
            int cuentaDestinoId,
            decimal monto);

        /// <summary>
        /// Valida si una cuenta tiene fondos suficientes para una transacción
        /// </summary>
        Task<bool> TieneFondosSuficientesAsync(int cuentaId, decimal monto);

        /// <summary>
        /// Calcula el crédito disponible de una tarjeta
        /// </summary>
        Task<decimal> CalcularCreditoDisponibleAsync(int tarjetaId);
    }
}
