using ArtemisBanking.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Application.Interfaces
{
    /// <summary>
    /// Interfaz que define todos los métodos para manejar transacciones
    /// Incluye: transferencias, pagos a tarjetas, pagos a préstamos, avances de efectivo
    /// </summary>
    public interface IServicioTransaccion
    {
        // ==================== MÉTODOS PRIVADOS (USADOS INTERNAMENTE) ====================

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

        // ==================== MÉTODOS PÚBLICOS (USADOS POR CONTROLADORES) ====================

        /// <summary>
        /// Realiza una transacción express (transferencia a cualquier cuenta)
        /// Este método es el que los controladores llamarán
        /// </summary>
        Task<ResultadoOperacion> RealizarTransaccionExpressAsync(TransaccionExpressDTO datos);

        /// <summary>
        /// Realiza un pago a tarjeta desde el cliente
        /// Este método es el que los controladores llamarán
        /// </summary>
        Task<ResultadoOperacion> PagarTarjetaCreditoClienteAsync(PagoTarjetaClienteDTO datos);

        /// <summary>
        /// Realiza un pago a préstamo desde el cliente
        /// Este método es el que los controladores llamarán
        /// </summary>
        Task<ResultadoOperacion> PagarPrestamoClienteAsync(PagoPrestamoClienteDTO datos);

        /// <summary>
        /// Realiza un pago a un beneficiario
        /// Primero obtiene los datos del beneficiario y luego hace la transferencia
        /// </summary>
        Task<ResultadoOperacion> PagarBeneficiarioAsync(PagoBeneficiarioDTO datos);

        /// <summary>
        /// Realiza un avance de efectivo desde tarjeta
        /// Este método es el que los controladores llamarán
        /// </summary>
        Task<ResultadoOperacion> RealizarAvanceEfectivoClienteAsync(AvanceEfectivoDTO datos);

        /// <summary>
        /// Obtiene información de confirmación de una cuenta destino
        /// Se usa para mostrar al usuario a quién le va a transferir antes de confirmar
        /// </summary>
        Task<ResultadoOperacion<(string nombre, string apellido)>> ObtenerInfoCuentaDestinoAsync(string numeroCuenta);
    }
}