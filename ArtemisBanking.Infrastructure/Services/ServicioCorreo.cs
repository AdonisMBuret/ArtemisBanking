using ArtemisBanking.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace ArtemisBanking.Infrastructure.Services
{
     
    /// Servicio para envío de correos electrónicos mediante SMTP de Gmail
     
    public class ServicioCorreo : IServicioCorreo
    {
        private readonly IConfiguration _configuration;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public ServicioCorreo(IConfiguration configuration)
        {
            _configuration = configuration;

            // Leer configuración de SMTP desde appsettings.json
            _smtpHost = _configuration["EmailSettings:SmtpHost"];
            _smtpPort = _configuration.GetValue<int>("EmailSettings:SmtpPort");
            _smtpUsername = _configuration["EmailSettings:SmtpUsername"];
            _smtpPassword = _configuration["EmailSettings:SmtpPassword"];
            _fromEmail = _configuration["EmailSettings:FromEmail"];
            _fromName = _configuration["EmailSettings:FromName"];
        }

         
        /// Método privado para enviar correos
         
        private async Task EnviarCorreoAsync(string destinatario, string asunto, string cuerpo)
        {
            try
            {
                using (var mensaje = new MailMessage())
                {
                    mensaje.From = new MailAddress(_fromEmail, _fromName);
                    mensaje.To.Add(destinatario);
                    mensaje.Subject = asunto;
                    mensaje.Body = cuerpo;
                    mensaje.IsBodyHtml = true;

                    using (var cliente = new SmtpClient(_smtpHost, _smtpPort))
                    {
                        cliente.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                        cliente.EnableSsl = true;
                        await cliente.SendMailAsync(mensaje);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log del error (podrías usar ILogger aquí)
                Console.WriteLine($"Error al enviar correo: {ex.Message}");
                throw;
            }
        }

        public async Task EnviarCorreoConfirmacionAsync(string correo, string nombreUsuario, string token)
        {
            var asunto = "Confirma tu cuenta - Artemis Banking";
            var cuerpo = $@"
                <h2>Bienvenido a Artemis Banking</h2>
                <p>Hola <strong>{nombreUsuario}</strong>,</p>
                <p>Para activar tu cuenta, usa el siguiente token de confirmación:</p>
                <div style='background-color: #f4f4f4; padding: 15px; margin: 20px 0; border-radius: 5px;'>
                    <code style='font-size: 16px; color: #333;'>{token}</code>
                </div>
                <p>Este token es válido por 24 horas.</p>
                <br>
                <p>Saludos,<br>Equipo de Artemis Banking</p>
            ";

            await EnviarCorreoAsync(correo, asunto, cuerpo);
        }

        public async Task EnviarCorreoReseteoContrasenaAsync(string correo, string nombreUsuario, string token)
        {
            var asunto = "Restablecer contraseña - Artemis Banking";
            var cuerpo = $@"
                <h2>Restablecimiento de contraseña</h2>
                <p>Hola <strong>{nombreUsuario}</strong>,</p>
                <p>Recibimos una solicitud para restablecer tu contraseña. Usa el siguiente token:</p>
                <div style='background-color: #f4f4f4; padding: 15px; margin: 20px 0; border-radius: 5px;'>
                    <code style='font-size: 16px; color: #333;'>{token}</code>
                </div>
                <p>Si no solicitaste este cambio, ignora este correo.</p>
                <br>
                <p>Saludos,<br>Equipo de Artemis Banking</p>
            ";

            await EnviarCorreoAsync(correo, asunto, cuerpo);
        }

        public async Task EnviarNotificacionPrestamoAprobadoAsync(
            string correo,
            string nombreCliente,
            decimal monto,
            int plazoMeses,
            decimal tasaInteres,
            decimal cuotaMensual)
        {
            var asunto = "¡Préstamo aprobado! - Artemis Banking";
            var cuerpo = $@"
                <h2>¡Felicidades! Tu préstamo ha sido aprobado</h2>
                <p>Hola <strong>{nombreCliente}</strong>,</p>
                <p>Te informamos que tu solicitud de préstamo ha sido <strong>aprobada</strong>.</p>
                <h3>Detalles del préstamo:</h3>
                <ul>
                    <li><strong>Monto aprobado:</strong> RD${monto:N2}</li>
                    <li><strong>Plazo:</strong> {plazoMeses} meses</li>
                    <li><strong>Tasa de interés:</strong> {tasaInteres}% anual</li>
                    <li><strong>Cuota mensual:</strong> RD${cuotaMensual:N2}</li>
                </ul>
                <p>El monto ha sido depositado en tu cuenta de ahorro principal.</p>
                <br>
                <p>Saludos,<br>Equipo de Artemis Banking</p>
            ";

            await EnviarCorreoAsync(correo, asunto, cuerpo);
        }

        public async Task EnviarNotificacionCambioTasaPrestamoAsync(
            string correo,
            string nombreCliente,
            decimal nuevaTasa,
            decimal nuevaCuota)
        {
            var asunto = "Actualización de tasa de interés - Artemis Banking";
            var cuerpo = $@"
                <h2>Actualización en tu préstamo</h2>
                <p>Hola <strong>{nombreCliente}</strong>,</p>
                <p>Te informamos que la tasa de interés de tu préstamo ha sido actualizada.</p>
                <h3>Nuevos términos:</h3>
                <ul>
                    <li><strong>Nueva tasa de interés:</strong> {nuevaTasa}% anual</li>
                    <li><strong>Nueva cuota mensual:</strong> RD${nuevaCuota:N2}</li>
                </ul>
                <p>Este cambio aplicará a partir de tu próxima cuota.</p>
                <br>
                <p>Saludos,<br>Equipo de Artemis Banking</p>
            ";

            await EnviarCorreoAsync(correo, asunto, cuerpo);
        }

        public async Task EnviarNotificacionCambioLimiteTarjetaAsync(
            string correo,
            string nombreCliente,
            string ultimosCuatroDigitos,
            decimal nuevoLimite)
        {
            var asunto = "Actualización de límite de crédito - Artemis Banking";
            var cuerpo = $@"
                <h2>Actualización en tu tarjeta de crédito</h2>
                <p>Hola <strong>{nombreCliente}</strong>,</p>
                <p>El límite de crédito de tu tarjeta terminada en <strong>{ultimosCuatroDigitos}</strong> ha sido modificado.</p>
                <p><strong>Nuevo límite de crédito:</strong> RD${nuevoLimite:N2}</p>
                <br>
                <p>Saludos,<br>Equipo de Artemis Banking</p>
            ";

            await EnviarCorreoAsync(correo, asunto, cuerpo);
        }

        public async Task EnviarNotificacionTransaccionRealizadaAsync(
            string correo,
            string nombreCliente,
            decimal monto,
            string ultimosCuatroDigitosCuentaDestino,
            DateTime fechaHora)
        {
            var asunto = $"Transacción realizada a cuenta {ultimosCuatroDigitosCuentaDestino} - Artemis Banking";
            var cuerpo = $@"
                <h2>Transacción exitosa</h2>
                <p>Hola <strong>{nombreCliente}</strong>,</p>
                <p>Se ha realizado una transacción desde tu cuenta.</p>
                <h3>Detalles:</h3>
                <ul>
                    <li><strong>Monto:</strong> RD${monto:N2}</li>
                    <li><strong>Cuenta destino:</strong> ****{ultimosCuatroDigitosCuentaDestino}</li>
                    <li><strong>Fecha y hora:</strong> {fechaHora:dd/MM/yyyy HH:mm}</li>
                </ul>
                <br>
                <p>Saludos,<br>Equipo de Artemis Banking</p>
            ";

            await EnviarCorreoAsync(correo, asunto, cuerpo);
        }

        public async Task EnviarNotificacionTransaccionRecibidaAsync(
            string correo,
            string nombreCliente,
            decimal monto,
            string ultimosCuatroDigitosCuentaOrigen,
            DateTime fechaHora)
        {
            var asunto = $"Transacción recibida desde cuenta {ultimosCuatroDigitosCuentaOrigen} - Artemis Banking";
            var cuerpo = $@"
                <h2>Has recibido una transacción</h2>
                <p>Hola <strong>{nombreCliente}</strong>,</p>
                <p>Se ha recibido una transacción en tu cuenta.</p>
                <h3>Detalles:</h3>
                <ul>
                    <li><strong>Monto:</strong> RD${monto:N2}</li>
                    <li><strong>Cuenta origen:</strong> ****{ultimosCuatroDigitosCuentaOrigen}</li>
                    <li><strong>Fecha y hora:</strong> {fechaHora:dd/MM/yyyy HH:mm}</li>
                </ul>
                <br>
                <p>Saludos,<br>Equipo de Artemis Banking</p>
            ";

            await EnviarCorreoAsync(correo, asunto, cuerpo);
        }

        public async Task EnviarNotificacionPagoTarjetaAsync(
            string correo,
            string nombreCliente,
            decimal monto,
            string ultimosCuatroDigitosTarjeta,
            string ultimosCuatroDigitosCuenta,
            DateTime fechaHora)
        {
            var asunto = $"Pago realizado a tarjeta {ultimosCuatroDigitosTarjeta} - Artemis Banking";
            var cuerpo = $@"
                <h2>Pago a tarjeta de crédito exitoso</h2>
                <p>Hola <strong>{nombreCliente}</strong>,</p>
                <p>Se ha realizado un pago a tu tarjeta de crédito.</p>
                <h3>Detalles:</h3>
                <ul>
                    <li><strong>Monto pagado:</strong> RD${monto:N2}</li>
                    <li><strong>Tarjeta:</strong> ****{ultimosCuatroDigitosTarjeta}</li>
                    <li><strong>Cuenta origen:</strong> ****{ultimosCuatroDigitosCuenta}</li>
                    <li><strong>Fecha y hora:</strong> {fechaHora:dd/MM/yyyy HH:mm}</li>
                </ul>
                <br>
                <p>Saludos,<br>Equipo de Artemis Banking</p>
            ";

            await EnviarCorreoAsync(correo, asunto, cuerpo);
        }

        public async Task EnviarNotificacionPagoPrestamoAsync(
            string correo,
            string nombreCliente,
            decimal monto,
            string numeroPrestamo,
            string ultimosCuatroDigitosCuenta,
            DateTime fechaHora)
        {
            var asunto = $"Pago realizado al préstamo {numeroPrestamo} - Artemis Banking";
            var cuerpo = $@"
                <h2>Pago a préstamo exitoso</h2>
                <p>Hola <strong>{nombreCliente}</strong>,</p>
                <p>Se ha realizado un pago a tu préstamo.</p>
                <h3>Detalles:</h3>
                <ul>
                    <li><strong>Monto pagado:</strong> RD${monto:N2}</li>
                    <li><strong>Préstamo:</strong> {numeroPrestamo}</li>
                    <li><strong>Cuenta origen:</strong> ****{ultimosCuatroDigitosCuenta}</li>
                    <li><strong>Fecha y hora:</strong> {fechaHora:dd/MM/yyyy HH:mm}</li>
                </ul>
                <br>
                <p>Saludos,<br>Equipo de Artemis Banking</p>
            ";

            await EnviarCorreoAsync(correo, asunto, cuerpo);
        }

        public async Task EnviarNotificacionAvanceEfectivoAsync(
            string correo,
            string nombreCliente,
            decimal monto,
            string ultimosCuatroDigitosTarjeta,
            string ultimosCuatroDigitosCuenta,
            DateTime fechaHora)
        {
            var asunto = $"Avance de efectivo desde tarjeta {ultimosCuatroDigitosTarjeta} - Artemis Banking";
            var cuerpo = $@"
                <h2>Avance de efectivo realizado</h2>
                <p>Hola <strong>{nombreCliente}</strong>,</p>
                <p>Se ha realizado un avance de efectivo desde tu tarjeta de crédito.</p>
                <h3>Detalles:</h3>
                <ul>
                    <li><strong>Monto del avance:</strong> RD${monto:N2}</li>
                    <li><strong>Tarjeta:</strong> ****{ultimosCuatroDigitosTarjeta}</li>
                    <li><strong>Cuenta destino:</strong> ****{ultimosCuatroDigitosCuenta}</li>
                    <li><strong>Fecha y hora:</strong> {fechaHora:dd/MM/yyyy HH:mm}</li>
                </ul>
                <p><em>Nota: Se ha aplicado un interés del 6.25% sobre el monto del avance.</em></p>
                <br>
                <p>Saludos,<br>Equipo de Artemis Banking</p>
            ";

            await EnviarCorreoAsync(correo, asunto, cuerpo);
        }

        public async Task EnviarNotificacionDepositoAsync(
            string correo,
            string nombreCliente,
            decimal monto,
            string ultimosCuatroDigitosCuenta,
            DateTime fechaHora)
        {
            var asunto = $"Depósito realizado a tu cuenta {ultimosCuatroDigitosCuenta} - Artemis Banking";
            var cuerpo = $@"
                <h2>Depósito exitoso</h2>
                <p>Hola <strong>{nombreCliente}</strong>,</p>
                <p>Se ha realizado un depósito en tu cuenta.</p>
                <h3>Detalles:</h3>
                <ul>
                    <li><strong>Monto depositado:</strong> RD${monto:N2}</li>
                    <li><strong>Cuenta:</strong> ****{ultimosCuatroDigitosCuenta}</li>
                    <li><strong>Fecha y hora:</strong> {fechaHora:dd/MM/yyyy HH:mm}</li>
                </ul>
                <br>
                <p>Saludos,<br>Equipo de Artemis Banking</p>
            ";

            await EnviarCorreoAsync(correo, asunto, cuerpo);
        }

        public async Task EnviarNotificacionRetiroAsync(
            string correo,
            string nombreCliente,
            decimal monto,
            string ultimosCuatroDigitosCuenta,
            DateTime fechaHora)
        {
            var asunto = $"Retiro realizado de tu cuenta {ultimosCuatroDigitosCuenta} - Artemis Banking";
            var cuerpo = $@"
                <h2>Retiro exitoso</h2>
                <p>Hola <strong>{nombreCliente}</strong>,</p>
                <p>Se ha realizado un retiro de tu cuenta.</p>
                <h3>Detalles:</h3>
                <ul>
                    <li><strong>Monto retirado:</strong> RD${monto:N2}</li>
                    <li><strong>Cuenta:</strong> ****{ultimosCuatroDigitosCuenta}</li>
                    <li><strong>Fecha y hora:</strong> {fechaHora:dd/MM/yyyy HH:mm}</li>
                </ul>
                <br>
                <p>Saludos,<br>Equipo de Artemis Banking</p>
            ";

            await EnviarCorreoAsync(correo, asunto, cuerpo);
        }

        public async Task EnviarNotificacionConsumoTarjetaAsync(
            string correo,
            string nombreCliente,
            decimal monto,
            string ultimosCuatroDigitosTarjeta,
            string nombreComercio,
            DateTime fechaHora)
        {
            var asunto = $"Consumo realizado con tarjeta {ultimosCuatroDigitosTarjeta} - Artemis Banking";
            var cuerpo = $@"
                <h2>Consumo realizado</h2>
                <p>Hola <strong>{nombreCliente}</strong>,</p>
                <p>Se ha realizado un consumo con tu tarjeta de crédito.</p>
                <h3>Detalles:</h3>
                <ul>
                    <li><strong>Monto:</strong> RD${monto:N2}</li>
                    <li><strong>Tarjeta:</strong> ****{ultimosCuatroDigitosTarjeta}</li>
                    <li><strong>Comercio:</strong> {nombreComercio}</li>
                    <li><strong>Fecha y hora:</strong> {fechaHora:dd/MM/yyyy HH:mm}</li>
                </ul>
                <br>
                <p>Saludos,<br>Equipo de Artemis Banking</p>
            ";

            await EnviarCorreoAsync(correo, asunto, cuerpo);
        }
    }
}