using ArtemisBanking.Application.DTOs;

namespace ArtemisBanking.Application.Interfaces
{
    /// Interfaz que define todos los métodos para manejar transacciones
    /// Incluye: transferencias, pagos a tarjetas, pagos a préstamos, avances de efectivo
     
    public interface IServicioTransaccion
    {
        // ==================== MÉTODOS PRIVADOS (USADOS INTERNAMENTE) ====================
         
        /// Realiza una transferencia entre dos cuentas de ahorro
        /// Valida fondos, actualiza balances y registra transacciones
         
        Task<(bool exito, string mensaje)> RealizarTransferenciaAsync(
            int cuentaOrigenId,
            string numeroCuentaDestino,
            decimal monto);

         
        /// Procesa un pago a una tarjeta de crédito desde una cuenta de ahorro
        /// Solo se paga hasta el monto de la deuda actual
         
        Task<(bool exito, string mensaje, decimal montoPagado)> PagarTarjetaCreditoAsync(
            int cuentaOrigenId,
            int tarjetaId,
            decimal monto);

         
        /// Procesa un pago a un préstamo desde una cuenta de ahorro
        /// Aplica el pago a las cuotas pendientes de forma secuencial
         
        Task<(bool exito, string mensaje, decimal montoAplicado)> PagarPrestamoAsync(
            int cuentaOrigenId,
            int prestamoId,
            decimal monto);

         
        /// Realiza un avance de efectivo desde una tarjeta a una cuenta
        /// Aplica un interés del 6.25% sobre el monto
         
        Task<(bool exito, string mensaje)> RealizarAvanceEfectivoAsync(
            int tarjetaId,
            int cuentaDestinoId,
            decimal monto);

         
        /// Valida si una cuenta tiene fondos suficientes para una transacción
         
        Task<bool> TieneFondosSuficientesAsync(int cuentaId, decimal monto);

         
        /// Calcula el crédito disponible de una tarjeta
         
        Task<decimal> CalcularCreditoDisponibleAsync(int tarjetaId);

        // ==================== MÉTODOS PÚBLICOS (USADOS POR CONTROLADORES) ====================
         
        /// Realiza una transacción express (transferencia a cualquier cuenta)
        /// Este método es el que los controladores llamarán
         
        Task<ResultadoOperacion> RealizarTransaccionExpressAsync(TransaccionExpressDTO datos);

         
        /// Realiza un pago a tarjeta desde el cliente
        /// Este método es el que los controladores llamarán
         
        Task<ResultadoOperacion> PagarTarjetaCreditoClienteAsync(PagoTarjetaClienteDTO datos);

         
        /// Realiza un pago a préstamo desde el cliente
        /// Este método es el que los controladores llamarán
         
        Task<ResultadoOperacion> PagarPrestamoClienteAsync(PagoPrestamoClienteDTO datos);

         
        /// Realiza un pago a un beneficiario
        /// Primero obtiene los datos del beneficiario y luego hace la transferencia
         
        Task<ResultadoOperacion> PagarBeneficiarioAsync(PagoBeneficiarioDTO datos);

         
        /// Realiza un avance de efectivo desde tarjeta
        /// Este método es el que los controladores llamarán
         
        Task<ResultadoOperacion> RealizarAvanceEfectivoClienteAsync(AvanceEfectivoDTO datos);

         
        /// Obtiene información de confirmación de una cuenta destino
        /// Se usa para mostrar al usuario a quién le va a transferir antes de confirmar
         
        Task<ResultadoOperacion<(string nombre, string apellido)>> ObtenerInfoCuentaDestinoAsync(string numeroCuenta);
    }
}