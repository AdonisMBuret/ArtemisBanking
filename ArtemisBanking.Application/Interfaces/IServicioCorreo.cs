namespace ArtemisBanking.Application.Interfaces
{
    /// Interfaz para el servicio de correo electrónico
    /// Define todos los métodos para enviar notificaciones por email
    public interface IServicioCorreo
    {
        Task EnviarCorreoConfirmacionAsync(string correo, string nombreUsuario, string usuarioId, string token, string urlBase);
        Task EnviarCorreoReseteoContrasenaAsync(string correo, string nombreUsuario, string usuarioId, string token, string urlBase);
        Task EnviarNotificacionPrestamoAprobadoAsync(string correo, string nombreCliente, decimal monto, int plazoMeses, decimal tasaInteres, decimal cuotaMensual);
        Task EnviarNotificacionCambioTasaPrestamoAsync(string correo, string nombreCliente, decimal nuevaTasa, decimal nuevaCuota);
        Task EnviarNotificacionCambioLimiteTarjetaAsync(string correo, string nombreCliente, string ultimosCuatroDigitos, decimal nuevoLimite);
        Task EnviarNotificacionTransaccionRealizadaAsync(string correo, string nombreCliente, decimal monto, string ultimosCuatroDigitosCuentaDestino, DateTime fechaHora);
        Task EnviarNotificacionTransaccionRecibidaAsync(string correo, string nombreCliente, decimal monto, string ultimosCuatroDigitosCuentaOrigen, DateTime fechaHora);
        Task EnviarNotificacionPagoTarjetaAsync(string correo, string nombreCliente, decimal monto, string ultimosCuatroDigitosTarjeta, string ultimosCuatroDigitosCuenta, DateTime fechaHora);
        Task EnviarNotificacionPagoPrestamoAsync(string correo, string nombreCliente, decimal monto, string numeroPrestamo, string ultimosCuatroDigitosCuenta, DateTime fechaHora);
        Task EnviarNotificacionAvanceEfectivoAsync(string correo, string nombreCliente, decimal monto, string ultimosCuatroDigitosTarjeta, string ultimosCuatroDigitosCuenta, DateTime fechaHora);
        Task EnviarNotificacionDepositoAsync(string correo, string nombreCliente, decimal monto, string ultimosCuatroDigitosCuenta, DateTime fechaHora);
        Task EnviarNotificacionRetiroAsync(string correo, string nombreCliente, decimal monto, string ultimosCuatroDigitosCuenta, DateTime fechaHora);
        Task EnviarNotificacionConsumoTarjetaAsync(string correo, string nombreCliente, decimal monto, string ultimosCuatroDigitosTarjeta, string nombreComercio, DateTime fechaHora);
    }
}