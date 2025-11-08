using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Application.Interfaces.Services
{
    public interface IServicioCorreo
    {
        // Enviar correo de confirmación de cuenta con token
        Task EnviarCorreoConfirmacionAsync(string correo, string nombreUsuario, string token);

        // Enviar correo para reseteo de contraseña
        Task EnviarCorreoReseteoContrasenaAsync(string correo, string nombreUsuario, string token);

        // Enviar notificación de préstamo aprobado
        Task EnviarNotificacionPrestamoAprobadoAsync(string correo, string nombreCliente, decimal monto, int plazoMeses, decimal tasaInteres, decimal cuotaMensual);

        // Enviar notificación de cambio de tasa de préstamo
        Task EnviarNotificacionCambioTasaPrestamoAsync(string correo, string nombreCliente, decimal nuevaTasa, decimal nuevaCuota);

        // Enviar notificación de cambio de límite de tarjeta
        Task EnviarNotificacionCambioLimiteTarjetaAsync(string correo, string nombreCliente, string ultimosCuatroDigitos, decimal nuevoLimite);

        // Enviar notificación de transacción realizada
        Task EnviarNotificacionTransaccionRealizadaAsync(string correo, string nombreCliente, decimal monto, string ultimosCuatroDigitosCuentaDestino, DateTime fechaHora);

        // Enviar notificación de transacción recibida
        Task EnviarNotificacionTransaccionRecibidaAsync(string correo, string nombreCliente, decimal monto, string ultimosCuatroDigitosCuentaOrigen, DateTime fechaHora);

        // Enviar notificación de pago a tarjeta
        Task EnviarNotificacionPagoTarjetaAsync(string correo, string nombreCliente, decimal monto, string ultimosCuatroDigitosTarjeta, string ultimosCuatroDigitosCuenta, DateTime fechaHora);

        // Enviar notificación de pago a préstamo
        Task EnviarNotificacionPagoPrestamoAsync(string correo, string nombreCliente, decimal monto, string numeroPrestamo, string ultimosCuatroDigitosCuenta, DateTime fechaHora);

        // Enviar notificación de avance de efectivo
        Task EnviarNotificacionAvanceEfectivoAsync(string correo, string nombreCliente, decimal monto, string ultimosCuatroDigitosTarjeta, string ultimosCuatroDigitosCuenta, DateTime fechaHora);

        // Enviar notificación de depósito
        Task EnviarNotificacionDepositoAsync(string correo, string nombreCliente, decimal monto, string ultimosCuatroDigitosCuenta, DateTime fechaHora);

        // Enviar notificación de retiro
        Task EnviarNotificacionRetiroAsync(string correo, string nombreCliente, decimal monto, string ultimosCuatroDigitosCuenta, DateTime fechaHora);

        // Enviar notificación de consumo en tarjeta
        Task EnviarNotificacionConsumoTarjetaAsync(string correo, string nombreCliente, decimal monto, string ultimosCuatroDigitosTarjeta, string nombreComercio, DateTime fechaHora);
    }
}
