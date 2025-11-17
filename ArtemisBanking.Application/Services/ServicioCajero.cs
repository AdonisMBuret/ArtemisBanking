using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace ArtemisBanking.Application.Services
{
    public class ServicioCajero : IServicioCajero
    {
        private readonly IRepositorioCuentaAhorro _repositorioCuenta;
        private readonly IRepositorioTarjetaCredito _repositorioTarjeta;
        private readonly IRepositorioPrestamo _repositorioPrestamo;
        private readonly IRepositorioCuotaPrestamo _repositorioCuotaPrestamo;
        private readonly IRepositorioTransaccion _repositorioTransaccion;
        private readonly IServicioCorreo _servicioCorreo;
        private readonly ILogger<ServicioCajero> _logger;

        public ServicioCajero(
            IRepositorioCuentaAhorro repositorioCuenta,
            IRepositorioTarjetaCredito repositorioTarjeta,
            IRepositorioPrestamo repositorioPrestamo,
            IRepositorioCuotaPrestamo repositorioCuotaPrestamo,
            IRepositorioTransaccion repositorioTransaccion,
            IServicioCorreo servicioCorreo,
            ILogger<ServicioCajero> logger)
        {
            _repositorioCuenta = repositorioCuenta;
            _repositorioTarjeta = repositorioTarjeta;
            _repositorioPrestamo = repositorioPrestamo;
            _repositorioCuotaPrestamo = repositorioCuotaPrestamo;
            _repositorioTransaccion = repositorioTransaccion;
            _servicioCorreo = servicioCorreo;
            _logger = logger;
        }

         
        /// Realiza un depósito a una cuenta de ahorro
         
        public async Task<ResultadoOperacion> RealizarDepositoAsync(DepositoCajeroDTO datos)
        {
            try
            {
                // 1. Validar que la cuenta existe y está activa
                var cuenta = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(datos.NumeroCuentaDestino);

                if (cuenta == null || !cuenta.EstaActiva)
                {
                    return ResultadoOperacion.Fallo("El número de cuenta ingresado no es válido o está inactiva");
                }

                // 2. Acreditar el monto a la cuenta
                cuenta.Balance += datos.Monto;
                await _repositorioCuenta.ActualizarAsync(cuenta);
                await _repositorioCuenta.GuardarCambiosAsync();

                // 3. Registrar la transacción
                var transaccion = new Transaccion
                {
                    FechaTransaccion = DateTime.Now,
                    Monto = datos.Monto,
                    TipoTransaccion = Constantes.TipoCredito,
                    Beneficiario = cuenta.NumeroCuenta,
                    Origen = Constantes.TextoDeposito,
                    EstadoTransaccion = Constantes.EstadoAprobada,
                    CuentaAhorroId = cuenta.Id,
                    FechaCreacion = DateTime.Now
                };

                await _repositorioTransaccion.AgregarAsync(transaccion);
                await _repositorioTransaccion.GuardarCambiosAsync();

                // 4. Enviar correo de notificación
                try
                {
                    cuenta = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(cuenta.NumeroCuenta);

                    await _servicioCorreo.EnviarNotificacionDepositoAsync(
                        cuenta.Usuario.Email,
                        cuenta.Usuario.NombreCompleto,
                        datos.Monto,
                        cuenta.NumeroCuenta.Substring(cuenta.NumeroCuenta.Length - 4),
                        DateTime.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al enviar correo de notificación de depósito");
                }

                _logger.LogInformation($"Depósito de RD${datos.Monto} realizado a cuenta {cuenta.NumeroCuenta} por cajero {datos.CajeroId}");

                return ResultadoOperacion.Ok("Depósito realizado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al realizar depósito");
                return ResultadoOperacion.Fallo("Error al procesar el depósito");
            }
        }

         
        /// Realiza un retiro de una cuenta de ahorro
         
        public async Task<ResultadoOperacion> RealizarRetiroAsync(RetiroCajeroDTO datos)
        {
            try
            {
                // 1. Validar que la cuenta existe y está activa
                var cuenta = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(datos.NumeroCuentaOrigen);

                if (cuenta == null || !cuenta.EstaActiva)
                {
                    return ResultadoOperacion.Fallo("El número de cuenta ingresado no es válido o está inactiva");
                }

                // 2. Validar fondos suficientes
                if (cuenta.Balance < datos.Monto)
                {
                    return ResultadoOperacion.Fallo("Fondos insuficientes en la cuenta");
                }

                // 3. Debitar el monto de la cuenta
                cuenta.Balance -= datos.Monto;
                await _repositorioCuenta.ActualizarAsync(cuenta);
                await _repositorioCuenta.GuardarCambiosAsync();

                // 4. Registrar la transacción
                var transaccion = new Transaccion
                {
                    FechaTransaccion = DateTime.Now,
                    Monto = datos.Monto,
                    TipoTransaccion = Constantes.TipoDebito,
                    Beneficiario = Constantes.TextoRetiro,
                    Origen = cuenta.NumeroCuenta,
                    EstadoTransaccion = Constantes.EstadoAprobada,
                    CuentaAhorroId = cuenta.Id,
                    FechaCreacion = DateTime.Now
                };

                await _repositorioTransaccion.AgregarAsync(transaccion);
                await _repositorioTransaccion.GuardarCambiosAsync();

                // 5. Enviar correo de notificación
                try
                {
                    cuenta = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(cuenta.NumeroCuenta);

                    await _servicioCorreo.EnviarNotificacionRetiroAsync(
                        cuenta.Usuario.Email,
                        cuenta.Usuario.NombreCompleto,
                        datos.Monto,
                        cuenta.NumeroCuenta.Substring(cuenta.NumeroCuenta.Length - 4),
                        DateTime.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al enviar correo de notificación de retiro");
                }

                _logger.LogInformation($"Retiro de RD${datos.Monto} realizado de cuenta {cuenta.NumeroCuenta} por cajero {datos.CajeroId}");

                return ResultadoOperacion.Ok("Retiro realizado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al realizar retiro");
                return ResultadoOperacion.Fallo("Error al procesar el retiro");
            }
        }

         
        /// Procesa un pago a tarjeta de crédito
         
        public async Task<ResultadoOperacion> PagarTarjetaCreditoAsync(PagoTarjetaCajeroDTO datos)
        {
            try
            {
                // 1. Validar cuenta y tarjeta
                var cuenta = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(datos.NumeroCuentaOrigen);
                var tarjeta = await _repositorioTarjeta.ObtenerPorNumeroTarjetaAsync(datos.NumeroTarjeta);

                if (cuenta == null || !cuenta.EstaActiva)
                {
                    return ResultadoOperacion.Fallo("El número de cuenta ingresado no es válido o está inactiva");
                }

                if (tarjeta == null || !tarjeta.EstaActiva)
                {
                    return ResultadoOperacion.Fallo("El número de tarjeta ingresado no es válido o está inactiva");
                }

                // 2. Validar fondos suficientes
                if (cuenta.Balance < datos.Monto)
                {
                    return ResultadoOperacion.Fallo("Fondos insuficientes en la cuenta");
                }

                // 3. Calcular monto real a pagar (no se puede pagar más que la deuda)
                decimal montoPagar = Math.Min(datos.Monto, tarjeta.DeudaActual);

                if (montoPagar <= 0)
                {
                    return ResultadoOperacion.Fallo("La tarjeta no tiene deuda pendiente");
                }

                // 4. Procesar el pago
                cuenta.Balance -= montoPagar;
                tarjeta.DeudaActual -= montoPagar;

                await _repositorioCuenta.ActualizarAsync(cuenta);
                await _repositorioTarjeta.ActualizarAsync(tarjeta);
                await _repositorioTarjeta.GuardarCambiosAsync();

                // 5. Registrar transacción
                var transaccion = new Transaccion
                {
                    FechaTransaccion = DateTime.Now,
                    Monto = montoPagar,
                    TipoTransaccion = Constantes.TipoDebito,
                    Beneficiario = tarjeta.NumeroTarjeta,
                    Origen = cuenta.NumeroCuenta,
                    EstadoTransaccion = Constantes.EstadoAprobada,
                    CuentaAhorroId = cuenta.Id,
                    FechaCreacion = DateTime.Now
                };

                await _repositorioTransaccion.AgregarAsync(transaccion);
                await _repositorioTransaccion.GuardarCambiosAsync();

                // 6. Enviar correo
                try
                {
                    cuenta = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(cuenta.NumeroCuenta);
                    tarjeta = await _repositorioTarjeta.ObtenerPorNumeroTarjetaAsync(tarjeta.NumeroTarjeta);

                    await _servicioCorreo.EnviarNotificacionPagoTarjetaAsync(
                        tarjeta.Cliente.Email,
                        tarjeta.Cliente.NombreCompleto,
                        montoPagar,
                        tarjeta.UltimosCuatroDigitos,
                        cuenta.NumeroCuenta.Substring(cuenta.NumeroCuenta.Length - 4),
                        DateTime.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al enviar correo de notificación");
                }

                _logger.LogInformation($"Pago de RD${montoPagar} a tarjeta ****{tarjeta.UltimosCuatroDigitos} por cajero {datos.CajeroId}");

                return ResultadoOperacion.Ok($"Pago realizado exitosamente. Monto pagado: RD${montoPagar:N2}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al pagar tarjeta de crédito");
                return ResultadoOperacion.Fallo("Error al procesar el pago");
            }
        }

         
        /// Procesa un pago a un préstamo
         
        public async Task<ResultadoOperacion> PagarPrestamoAsync(PagoPrestamoCajeroDTO datos)
        {
            try
            {
                // 1. Validar cuenta y préstamo
                var cuenta = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(datos.NumeroCuentaOrigen);
                var prestamo = await _repositorioPrestamo.ObtenerPorNumeroPrestamoAsync(datos.NumeroPrestamo);

                if (cuenta == null || !cuenta.EstaActiva)
                {
                    return ResultadoOperacion.Fallo("El número de cuenta ingresado no es válido o está inactiva");
                }

                if (prestamo == null || !prestamo.EstaActivo)
                {
                    return ResultadoOperacion.Fallo("El número de préstamo ingresado no es válido o está completado");
                }

                // 2. Validar fondos suficientes
                if (cuenta.Balance < datos.Monto)
                {
                    return ResultadoOperacion.Fallo("Fondos insuficientes en la cuenta");
                }

                // 3. Aplicar pago a las cuotas pendientes de forma secuencial
                decimal montoRestante = datos.Monto;
                int cuotasPagadas = 0;

                while (montoRestante > 0)
                {
                    var cuotaPendiente = await _repositorioCuotaPrestamo.ObtenerPrimeraCuotaPendienteAsync(prestamo.Id);

                    if (cuotaPendiente == null)
                        break; // No hay más cuotas pendientes

                    if (montoRestante >= cuotaPendiente.MontoCuota)
                    {
                        montoRestante -= cuotaPendiente.MontoCuota;
                        cuotaPendiente.EstaPagada = true;
                        cuotaPendiente.EstaAtrasada = false;
                        await _repositorioCuotaPrestamo.ActualizarAsync(cuotaPendiente);
                        cuotasPagadas++;
                    }
                    else
                    {
                        break; // No se permite pago parcial de cuotas
                    }
                }

                await _repositorioCuotaPrestamo.GuardarCambiosAsync();

                // 4. Calcular cuánto dinero realmente se usó
                decimal montoUsado = datos.Monto - montoRestante;

                if (montoUsado <= 0)
                {
                    return ResultadoOperacion.Fallo("No se pudo aplicar el pago a ninguna cuota");
                }

                // 5. Descontar de la cuenta
                cuenta.Balance -= montoUsado;
                await _repositorioCuenta.ActualizarAsync(cuenta);
                await _repositorioCuenta.GuardarCambiosAsync();

                // 6. Verificar si el préstamo está completamente pagado
                var cuotasPendientesTotal = await _repositorioCuotaPrestamo.ObtenerCuotasDePrestamoAsync(prestamo.Id);
                if (cuotasPendientesTotal.All(c => c.EstaPagada))
                {
                    prestamo.EstaActivo = false;
                    await _repositorioPrestamo.ActualizarAsync(prestamo);
                    await _repositorioPrestamo.GuardarCambiosAsync();
                }

                // 7. Registrar transacción
                var transaccion = new Transaccion
                {
                    FechaTransaccion = DateTime.Now,
                    Monto = montoUsado,
                    TipoTransaccion = Constantes.TipoDebito,
                    Beneficiario = prestamo.NumeroPrestamo,
                    Origen = cuenta.NumeroCuenta,
                    EstadoTransaccion = Constantes.EstadoAprobada,
                    CuentaAhorroId = cuenta.Id,
                    FechaCreacion = DateTime.Now
                };

                await _repositorioTransaccion.AgregarAsync(transaccion);
                await _repositorioTransaccion.GuardarCambiosAsync();

                // 8. Enviar correo
                try
                {
                    cuenta = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(cuenta.NumeroCuenta);
                    prestamo = await _repositorioPrestamo.ObtenerPorNumeroPrestamoAsync(prestamo.NumeroPrestamo);

                    await _servicioCorreo.EnviarNotificacionPagoPrestamoAsync(
                        prestamo.Cliente.Email,
                        prestamo.Cliente.NombreCompleto,
                        montoUsado,
                        prestamo.NumeroPrestamo,
                        cuenta.NumeroCuenta.Substring(cuenta.NumeroCuenta.Length - 4),
                        DateTime.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al enviar correo de notificación");
                }

                var mensaje = $"Pago exitoso. Se pagaron {cuotasPagadas} cuota(s) del préstamo.";
                if (montoRestante > 0)
                    mensaje += $" Se devolvieron RD${montoRestante:N2} a su cuenta.";

                _logger.LogInformation($"Pago de RD${montoUsado} a préstamo {prestamo.NumeroPrestamo} por cajero {datos.CajeroId}");

                return ResultadoOperacion.Ok(mensaje);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al pagar préstamo");
                return ResultadoOperacion.Fallo("Error al procesar el pago");
            }
        }

         
        /// Realiza una transacción entre dos cuentas de terceros
         
        public async Task<ResultadoOperacion> TransaccionEntreTercerosAsync(TransaccionTercerosCajeroDTO datos)
        {
            try
            {
                // 1. Obtener ambas cuentas
                var cuentaOrigen = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(datos.NumeroCuentaOrigen);
                var cuentaDestino = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(datos.NumeroCuentaDestino);

                // 2. Validaciones
                if (cuentaOrigen == null || !cuentaOrigen.EstaActiva)
                {
                    return ResultadoOperacion.Fallo("El número de cuenta origen no es válido o está inactiva");
                }

                if (cuentaDestino == null || !cuentaDestino.EstaActiva)
                {
                    return ResultadoOperacion.Fallo("El número de cuenta destino no es válido o está inactiva");
                }

                if (cuentaOrigen.Balance < datos.Monto)
                {
                    return ResultadoOperacion.Fallo("Fondos insuficientes en la cuenta de origen");
                }

                // 3. Realizar débito y crédito
                cuentaOrigen.Balance -= datos.Monto;
                cuentaDestino.Balance += datos.Monto;

                await _repositorioCuenta.ActualizarAsync(cuentaOrigen);
                await _repositorioCuenta.ActualizarAsync(cuentaDestino);
                await _repositorioCuenta.GuardarCambiosAsync();

                // 4. Registrar transacciones en ambas cuentas
                var transaccionOrigen = new Transaccion
                {
                    FechaTransaccion = DateTime.Now,
                    Monto = datos.Monto,
                    TipoTransaccion = Constantes.TipoDebito,
                    Beneficiario = cuentaDestino.NumeroCuenta,
                    Origen = cuentaOrigen.NumeroCuenta,
                    EstadoTransaccion = Constantes.EstadoAprobada,
                    CuentaAhorroId = cuentaOrigen.Id,
                    FechaCreacion = DateTime.Now
                };

                var transaccionDestino = new Transaccion
                {
                    FechaTransaccion = DateTime.Now,
                    Monto = datos.Monto,
                    TipoTransaccion = Constantes.TipoCredito,
                    Beneficiario = cuentaDestino.NumeroCuenta,
                    Origen = cuentaOrigen.NumeroCuenta,
                    EstadoTransaccion = Constantes.EstadoAprobada,
                    CuentaAhorroId = cuentaDestino.Id,
                    FechaCreacion = DateTime.Now
                };

                await _repositorioTransaccion.AgregarAsync(transaccionOrigen);
                await _repositorioTransaccion.AgregarAsync(transaccionDestino);
                await _repositorioTransaccion.GuardarCambiosAsync();

                // 5. Enviar correos
                try
                {
                    cuentaOrigen = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(cuentaOrigen.NumeroCuenta);
                    cuentaDestino = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(cuentaDestino.NumeroCuenta);

                    var ultimos4Destino = cuentaDestino.NumeroCuenta.Substring(cuentaDestino.NumeroCuenta.Length - 4);
                    var ultimos4Origen = cuentaOrigen.NumeroCuenta.Substring(cuentaOrigen.NumeroCuenta.Length - 4);

                    await _servicioCorreo.EnviarNotificacionTransaccionRealizadaAsync(
                        cuentaOrigen.Usuario.Email,
                        cuentaOrigen.Usuario.NombreCompleto,
                        datos.Monto,
                        ultimos4Destino,
                        DateTime.Now);

                    await _servicioCorreo.EnviarNotificacionTransaccionRecibidaAsync(
                        cuentaDestino.Usuario.Email,
                        cuentaDestino.Usuario.NombreCompleto,
                        datos.Monto,
                        ultimos4Origen,
                        DateTime.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al enviar correos de notificación");
                }

                _logger.LogInformation($"Transacción de RD${datos.Monto} de {cuentaOrigen.NumeroCuenta} a {cuentaDestino.NumeroCuenta} por cajero {datos.CajeroId}");

                return ResultadoOperacion.Ok("Transacción realizada exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al realizar transacción entre terceros");
                return ResultadoOperacion.Fallo("Error al procesar la transacción");
            }
        }

         
        /// Obtiene el dashboard del cajero con las estadísticas del día
         
        public async Task<ResultadoOperacion<DashboardCajeroDTO>> ObtenerDashboardAsync(string cajeroId)
        {
            try
            {
                var dashboard = new DashboardCajeroDTO
                {
                    TransaccionesDelDia = await _repositorioTransaccion.ContarTransaccionesDelDiaAsync(),
                    PagosDelDia = await _repositorioTransaccion.ContarPagosDelDiaAsync(),
                    DepositosDelDia = await _repositorioTransaccion.ContarDepositosDelDiaAsync(),
                    RetirosDelDia = await _repositorioTransaccion.ContarRetirosDelDiaAsync()
                };

                return ResultadoOperacion<DashboardCajeroDTO>.Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener dashboard del cajero");
                return ResultadoOperacion<DashboardCajeroDTO>.Fallo("Error al cargar el dashboard");
            }
        }
    }
}
