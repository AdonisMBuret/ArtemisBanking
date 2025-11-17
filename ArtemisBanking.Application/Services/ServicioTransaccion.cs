using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace ArtemisBanking.Application.Services
{
    public class ServicioTransaccion : IServicioTransaccion
    {
        private readonly IRepositorioCuentaAhorro _repositorioCuenta;
        private readonly IRepositorioTarjetaCredito _repositorioTarjeta;
        private readonly IRepositorioPrestamo _repositorioPrestamo;
        private readonly IRepositorioCuotaPrestamo _repositorioCuotaPrestamo;
        private readonly IRepositorioTransaccion _repositorioTransaccion;
        private readonly IRepositorioConsumoTarjeta _repositorioConsumoTarjeta;
        private readonly IServicioCorreo _servicioCorreo;
        private readonly ILogger<ServicioTransaccion> _logger;
        private readonly IRepositorioBeneficiario _repositorioBeneficiario;

        public ServicioTransaccion(
            IRepositorioCuentaAhorro repositorioCuenta,
            IRepositorioTarjetaCredito repositorioTarjeta,
            IRepositorioPrestamo repositorioPrestamo,
            IRepositorioCuotaPrestamo repositorioCuotaPrestamo,
            IRepositorioTransaccion repositorioTransaccion,
            IRepositorioConsumoTarjeta repositorioConsumoTarjeta,
            IServicioCorreo servicioCorreo,
            IRepositorioBeneficiario repositorioBeneficiario,
            ILogger<ServicioTransaccion> logger)
        {
            _repositorioCuenta = repositorioCuenta;
            _repositorioTarjeta = repositorioTarjeta;
            _repositorioPrestamo = repositorioPrestamo;
            _repositorioCuotaPrestamo = repositorioCuotaPrestamo;
            _repositorioTransaccion = repositorioTransaccion;
            _repositorioConsumoTarjeta = repositorioConsumoTarjeta;
            _servicioCorreo = servicioCorreo;
            _repositorioBeneficiario = repositorioBeneficiario;
            _logger = logger;
        }

         
        /// Realiza una transferencia entre dos cuentas
        /// Maneja toda la lógica: validaciones, débito, crédito, transacciones y correos
         
        public async Task<(bool exito, string mensaje)> RealizarTransferenciaAsync(
            int cuentaOrigenId,
            string numeroCuentaDestino,
            decimal monto)
        {
            try
            {
                // 1. Obtener cuentas
                var cuentaOrigen = await _repositorioCuenta.ObtenerPorIdAsync(cuentaOrigenId);
                var cuentaDestino = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(numeroCuentaDestino);

                // 2. Validaciones
                if (cuentaOrigen == null || !cuentaOrigen.EstaActiva)
                    return (false, "La cuenta de origen no es válida.");

                if (cuentaDestino == null || !cuentaDestino.EstaActiva)
                    return (false, "La cuenta de destino no es válida.");

                if (cuentaOrigen.Balance < monto)
                    return (false, "Fondos insuficientes en la cuenta de origen.");

                // 3. Realizar débito y crédito
                cuentaOrigen.Balance -= monto;
                cuentaDestino.Balance += monto;

                await _repositorioCuenta.ActualizarAsync(cuentaOrigen);
                await _repositorioCuenta.ActualizarAsync(cuentaDestino);
                await _repositorioCuenta.GuardarCambiosAsync();

                // 4. Registrar transacciones en ambas cuentas
                var transaccionOrigen = new Transaccion
                {
                    FechaTransaccion = DateTime.Now,
                    Monto = monto,
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
                    Monto = monto,
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

                // 5. Enviar correos (opcional - puede fallar sin afectar la transacción)
                try
                {
                    var ultimos4Destino = cuentaDestino.NumeroCuenta.Substring(cuentaDestino.NumeroCuenta.Length - 4);
                    var ultimos4Origen = cuentaOrigen.NumeroCuenta.Substring(cuentaOrigen.NumeroCuenta.Length - 4);

                    // Cargar usuarios para obtener info completa
                    cuentaOrigen = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(cuentaOrigen.NumeroCuenta);
                    cuentaDestino = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(cuentaDestino.NumeroCuenta);

                    await _servicioCorreo.EnviarNotificacionTransaccionRealizadaAsync(
                        cuentaOrigen.Usuario.Email,
                        cuentaOrigen.Usuario.NombreCompleto,
                        monto,
                        ultimos4Destino,
                        DateTime.Now);

                    await _servicioCorreo.EnviarNotificacionTransaccionRecibidaAsync(
                        cuentaDestino.Usuario.Email,
                        cuentaDestino.Usuario.NombreCompleto,
                        monto,
                        ultimos4Origen,
                        DateTime.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al enviar correos de notificación, pero la transacción fue exitosa");
                }

                _logger.LogInformation($"Transferencia exitosa: RD${monto} de {cuentaOrigen.NumeroCuenta} a {cuentaDestino.NumeroCuenta}");
                return (true, "Transferencia realizada exitosamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al realizar transferencia");
                return (false, "Ocurrió un error al procesar la transferencia.");
            }
        }

         
        /// Procesa un pago a una tarjeta de crédito
        /// Solo se paga hasta el monto de la deuda (no se puede pagar de más)
         
        public async Task<(bool exito, string mensaje, decimal montoPagado)> PagarTarjetaCreditoAsync(
            int cuentaOrigenId,
            int tarjetaId,
            decimal monto)
        {
            try
            {
                // 1. Obtener cuenta y tarjeta
                var cuenta = await _repositorioCuenta.ObtenerPorIdAsync(cuentaOrigenId);
                var tarjeta = await _repositorioTarjeta.ObtenerPorIdAsync(tarjetaId);

                // 2. Validaciones
                if (cuenta == null || !cuenta.EstaActiva)
                    return (false, "La cuenta no es válida.", 0);

                if (tarjeta == null || !tarjeta.EstaActiva)
                    return (false, "La tarjeta no es válida.", 0);

                if (cuenta.Balance < monto)
                    return (false, "Fondos insuficientes.", 0);

                // 3. Calcular monto real a pagar (no se puede pagar más que la deuda)
                decimal montoPagar = Math.Min(monto, tarjeta.DeudaActual);

                if (montoPagar <= 0)
                    return (false, "La tarjeta no tiene deuda pendiente.", 0);

                // 4. Descontar de cuenta y reducir deuda de tarjeta
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
                    _logger.LogWarning(ex, "Error al enviar correo de notificación de pago");
                }

                _logger.LogInformation($"Pago a tarjeta exitoso: RD${montoPagar} a tarjeta ****{tarjeta.UltimosCuatroDigitos}");
                return (true, "Pago realizado exitosamente.", montoPagar);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al pagar tarjeta de crédito");
                return (false, "Ocurrió un error al procesar el pago.", 0);
            }
        }

         
        /// Procesa un pago a un préstamo
        /// Aplica el pago a las cuotas pendientes de forma secuencial
        /// Si sobra dinero después de pagar todas las cuotas, se devuelve a la cuenta
         
        public async Task<(bool exito, string mensaje, decimal montoAplicado)> PagarPrestamoAsync(
            int cuentaOrigenId,
            int prestamoId,
            decimal monto)
        {
            try
            {
                // 1. Obtener cuenta y préstamo
                var cuenta = await _repositorioCuenta.ObtenerPorIdAsync(cuentaOrigenId);
                var prestamo = await _repositorioPrestamo.ObtenerPorIdAsync(prestamoId);

                // 2. Validaciones
                if (cuenta == null || !cuenta.EstaActiva)
                    return (false, "La cuenta no es válida.", 0);

                if (prestamo == null || !prestamo.EstaActivo)
                    return (false, "El préstamo no es válido.", 0);

                if (cuenta.Balance < monto)
                    return (false, "Fondos insuficientes.", 0);

                // 3. Aplicar pago a las cuotas pendientes
                decimal montoRestante = monto;
                int cuotasPagadas = 0;

                while (montoRestante > 0)
                {
                    // Obtener la primera cuota pendiente
                    var cuotaPendiente = await _repositorioCuotaPrestamo.ObtenerPrimeraCuotaPendienteAsync(prestamoId);

                    if (cuotaPendiente == null)
                        break; // No hay más cuotas pendientes

                    // Si tenemos suficiente para pagar la cuota completa
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
                        // No tenemos suficiente para pagar la cuota completa
                        // En este sistema NO se permiten pagos parciales
                        break;
                    }
                }

                await _repositorioCuotaPrestamo.GuardarCambiosAsync();

                // 4. Calcular cuánto dinero realmente se usó
                decimal montoUsado = monto - montoRestante;

                if (montoUsado <= 0)
                    return (false, "No se pudo aplicar el pago a ninguna cuota.", 0);

                // 5. Descontar de la cuenta solo lo que se usó
                cuenta.Balance -= montoUsado;

                // Si sobró dinero, se queda en la cuenta (no se debita)
                await _repositorioCuenta.ActualizarAsync(cuenta);
                await _repositorioCuenta.GuardarCambiosAsync();

                // 6. Verificar si el préstamo está completamente pagado
                var cuotasPendientesTotal = await _repositorioCuotaPrestamo.ObtenerCuotasDePrestamoAsync(prestamoId);
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
                    _logger.LogWarning(ex, "Error al enviar correo de notificación de pago");
                }

                var mensaje = $"Pago exitoso. Se pagaron {cuotasPagadas} cuota(s) del préstamo.";
                if (montoRestante > 0)
                    mensaje += $" Se devolvieron RD${montoRestante:N2} a su cuenta.";

                _logger.LogInformation($"Pago a préstamo exitoso: RD${montoUsado} a préstamo {prestamo.NumeroPrestamo}");
                return (true, mensaje, montoUsado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al pagar préstamo");
                return (false, "Ocurrió un error al procesar el pago.", 0);
            }
        }

         
        /// Realiza un avance de efectivo desde una tarjeta a una cuenta
        /// Se aplica un interés del 6.25% sobre el monto
         
        public async Task<(bool exito, string mensaje)> RealizarAvanceEfectivoAsync(
            int tarjetaId,
            int cuentaDestinoId,
            decimal monto)
        {
            try
            {
                // 1. Obtener tarjeta y cuenta
                var tarjeta = await _repositorioTarjeta.ObtenerPorIdAsync(tarjetaId);
                var cuenta = await _repositorioCuenta.ObtenerPorIdAsync(cuentaDestinoId);

                // 2. Validaciones
                if (tarjeta == null || !tarjeta.EstaActiva)
                    return (false, "La tarjeta no es válida.");

                if (cuenta == null || !cuenta.EstaActiva)
                    return (false, "La cuenta no es válida.");

                // 3. Validar crédito disponible
                if (monto > tarjeta.CreditoDisponible)
                    return (false, $"El monto excede el crédito disponible (RD${tarjeta.CreditoDisponible:N2}).");

                // 4. Calcular interés (6.25%)
                decimal interes = monto * (Constantes.InteresAvanceEfectivo / 100);
                decimal deudaTotal = monto + interes;

                // 5. Acreditar a la cuenta y aumentar deuda de tarjeta
                cuenta.Balance += monto;
                tarjeta.DeudaActual += deudaTotal;

                await _repositorioCuenta.ActualizarAsync(cuenta);
                await _repositorioTarjeta.ActualizarAsync(tarjeta);
                await _repositorioTarjeta.GuardarCambiosAsync();

                // 6. Registrar transacción en cuenta
                var transaccion = new Transaccion
                {
                    FechaTransaccion = DateTime.Now,
                    Monto = monto,
                    TipoTransaccion = Constantes.TipoCredito,
                    Beneficiario = cuenta.NumeroCuenta,
                    Origen = tarjeta.UltimosCuatroDigitos,
                    EstadoTransaccion = Constantes.EstadoAprobada,
                    CuentaAhorroId = cuenta.Id,
                    FechaCreacion = DateTime.Now
                };

                await _repositorioTransaccion.AgregarAsync(transaccion);
                await _repositorioTransaccion.GuardarCambiosAsync();

                // 7. Registrar consumo en tarjeta
                var consumo = new ConsumoTarjeta
                {
                    FechaConsumo = DateTime.Now,
                    Monto = deudaTotal,
                    NombreComercio = Constantes.TextoAvance,
                    EstadoConsumo = Constantes.ConsumoAprobado,
                    TarjetaId = tarjeta.Id,
                    FechaCreacion = DateTime.Now
                };

                await _repositorioConsumoTarjeta.AgregarAsync(consumo);
                await _repositorioConsumoTarjeta.GuardarCambiosAsync();

                // 8. Enviar correo
                try
                {
                    cuenta = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(cuenta.NumeroCuenta);
                    tarjeta = await _repositorioTarjeta.ObtenerPorNumeroTarjetaAsync(tarjeta.NumeroTarjeta);

                    await _servicioCorreo.EnviarNotificacionAvanceEfectivoAsync(
                        tarjeta.Cliente.Email,
                        tarjeta.Cliente.NombreCompleto,
                        monto,
                        tarjeta.UltimosCuatroDigitos,
                        cuenta.NumeroCuenta.Substring(cuenta.NumeroCuenta.Length - 4),
                        DateTime.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al enviar correo de notificación");
                }

                _logger.LogInformation($"Avance de efectivo exitoso: RD${monto} (Interés: RD${interes})");
                return (true, $"Avance realizado. Interés aplicado: RD${interes:N2}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al realizar avance de efectivo");
                return (false, "Ocurrió un error al procesar el avance de efectivo.");
            }
        }

         
        /// Valida si una cuenta tiene fondos suficientes para una operación
         
        public async Task<bool> TieneFondosSuficientesAsync(int cuentaId, decimal monto)
        {
            var cuenta = await _repositorioCuenta.ObtenerPorIdAsync(cuentaId);
            return cuenta != null && cuenta.EstaActiva && cuenta.Balance >= monto;
        }

         
        /// Calcula el crédito disponible de una tarjeta
         
        public async Task<decimal> CalcularCreditoDisponibleAsync(int tarjetaId)
        {
            var tarjeta = await _repositorioTarjeta.ObtenerPorIdAsync(tarjetaId);
            return tarjeta?.CreditoDisponible ?? 0;
        }

         
        /// Realiza una transacción express (transferencia a cualquier cuenta)
        /// Este método es el que los controladores llamarán
         
        public async Task<ResultadoOperacion> RealizarTransaccionExpressAsync(TransaccionExpressDTO datos)
        {
            try
            {
                // Llamar al método privado existente
                var (exito, mensaje) = await RealizarTransferenciaAsync(
                    datos.CuentaOrigenId,
                    datos.NumeroCuentaDestino,
                    datos.Monto);

                if (exito)
                    return ResultadoOperacion.Ok(mensaje);

                return ResultadoOperacion.Fallo(mensaje);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al realizar transacción express");
                return ResultadoOperacion.Fallo("Error al procesar la transacción");
            }
        }

         
        /// Realiza un pago a tarjeta desde el cliente
        /// Este método es el que los controladores llamarán
         
        public async Task<ResultadoOperacion> PagarTarjetaCreditoClienteAsync(PagoTarjetaClienteDTO datos)
        {
            try
            {
                // Llamar al método privado existente
                var (exito, mensaje, montoPagado) = await PagarTarjetaCreditoAsync(
                    datos.CuentaOrigenId,
                    datos.TarjetaId,
                    datos.Monto);

                if (exito)
                    return ResultadoOperacion.Ok($"{mensaje} Monto pagado: RD${montoPagado:N2}");

                return ResultadoOperacion.Fallo(mensaje);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al pagar tarjeta de crédito");
                return ResultadoOperacion.Fallo("Error al procesar el pago");
            }
        }

         
        /// Realiza un pago a préstamo desde el cliente
        /// Este método es el que los controladores llamarán
         
        public async Task<ResultadoOperacion> PagarPrestamoClienteAsync(PagoPrestamoClienteDTO datos)
        {
            try
            {
                // Llamar al método privado existente
                var (exito, mensaje, montoAplicado) = await PagarPrestamoAsync(
                    datos.CuentaOrigenId,
                    datos.PrestamoId,
                    datos.Monto);

                if (exito)
                    return ResultadoOperacion.Ok(mensaje);

                return ResultadoOperacion.Fallo(mensaje);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al pagar préstamo");
                return ResultadoOperacion.Fallo("Error al procesar el pago");
            }
        }

         
        /// Realiza un pago a un beneficiario
        /// Primero obtiene los datos del beneficiario y luego hace la transferencia
         
        public async Task<ResultadoOperacion> PagarBeneficiarioAsync(PagoBeneficiarioDTO datos)
        {
            try
            {
                // 1. Obtener el beneficiario
                var beneficiario = await _repositorioBeneficiario.ObtenerPorIdAsync(datos.BeneficiarioId);

                if (beneficiario == null)
                    return ResultadoOperacion.Fallo("Beneficiario no encontrado");

                // 2. Validar que el beneficiario pertenezca al usuario
                if (beneficiario.UsuarioId != datos.UsuarioId)
                    return ResultadoOperacion.Fallo("No tiene permiso para usar este beneficiario");

                // 3. Realizar la transferencia usando el método existente
                var (exito, mensaje) = await RealizarTransferenciaAsync(
                    datos.CuentaOrigenId,
                    beneficiario.NumeroCuentaBeneficiario,
                    datos.Monto);

                if (exito)
                    return ResultadoOperacion.Ok(mensaje);

                return ResultadoOperacion.Fallo(mensaje);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al pagar beneficiario");
                return ResultadoOperacion.Fallo("Error al procesar el pago");
            }
        }

         
        /// Realiza un avance de efectivo desde tarjeta
        /// Este método es el que los controladores llamarán
         
        public async Task<ResultadoOperacion> RealizarAvanceEfectivoClienteAsync(AvanceEfectivoDTO datos)
        {
            try
            {
                // Llamar al método privado existente
                var (exito, mensaje) = await RealizarAvanceEfectivoAsync(
                    datos.TarjetaId,
                    datos.CuentaDestinoId,
                    datos.Monto);

                if (exito)
                    return ResultadoOperacion.Ok(mensaje);

                return ResultadoOperacion.Fallo(mensaje);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al realizar avance de efectivo");
                return ResultadoOperacion.Fallo("Error al procesar el avance");
            }
        }

         
        /// Obtiene información de confirmación de una cuenta destino
        /// Se usa para mostrar al usuario a quién le va a transferir antes de confirmar
         
        public async Task<ResultadoOperacion<(string nombre, string apellido)>> ObtenerInfoCuentaDestinoAsync(string numeroCuenta)
        {
            try
            {
                var cuenta = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(numeroCuenta);

                if (cuenta == null || !cuenta.EstaActiva)
                    return ResultadoOperacion<(string, string)>.Fallo("Cuenta no válida");

                return ResultadoOperacion<(string, string)>.Ok(
                    (cuenta.Usuario.Nombre, cuenta.Usuario.Apellido));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener info de cuenta destino");
                return ResultadoOperacion<(string, string)>.Fallo("Error al obtener información");
            }
        }
    }
}
