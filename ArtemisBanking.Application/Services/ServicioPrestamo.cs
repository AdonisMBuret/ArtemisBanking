using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Domain.Interfaces.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Application.Services
{
    public class ServicioPrestamo : IServicioPrestamo
    {
        private readonly IRepositorioPrestamo _repositorioPrestamo;
        private readonly IRepositorioCuentaAhorro _repositorioCuenta;
        private readonly IRepositorioCuotaPrestamo _repositorioCuota;
        private readonly IRepositorioTransaccion _repositorioTransaccion;
        private readonly IServicioCalculoPrestamo _servicioCalculo;
        private readonly IServicioCorreo _servicioCorreo;
        private readonly UserManager<Usuario> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<ServicioPrestamo> _logger;

        public ServicioPrestamo(
            IRepositorioPrestamo repositorioPrestamo,
            IRepositorioCuentaAhorro repositorioCuenta,
            IRepositorioCuotaPrestamo repositorioCuota,
            IRepositorioTransaccion repositorioTransaccion,
            IServicioCalculoPrestamo servicioCalculo,
            IServicioCorreo servicioCorreo,
            UserManager<Usuario> userManager,
            IMapper mapper,
            ILogger<ServicioPrestamo> logger)
        {
            _repositorioPrestamo = repositorioPrestamo;
            _repositorioCuenta = repositorioCuenta;
            _repositorioCuota = repositorioCuota;
            _repositorioTransaccion = repositorioTransaccion;
            _servicioCalculo = servicioCalculo;
            _servicioCorreo = servicioCorreo;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResultadoOperacion<PrestamoDTO>> AsignarPrestamoAsync(AsignarPrestamoDTO datos)
        {
            try
            {
                // 1. Validar que el cliente existe
                var cliente = await _userManager.FindByIdAsync(datos.ClienteId);
                if (cliente == null)
                    return ResultadoOperacion<PrestamoDTO>.Fallo("Cliente no encontrado");

                // 2. Validar que no tenga préstamo activo
                var tienePrestamoActivo = await _repositorioPrestamo.TienePrestamoActivoAsync(datos.ClienteId);
                if (tienePrestamoActivo)
                    return ResultadoOperacion<PrestamoDTO>.Fallo("El cliente ya tiene un préstamo activo");

                // 3. Generar número de préstamo único
                var numeroPrestamo = await _repositorioPrestamo.GenerarNumeroPrestamoUnicoAsync();

                // 4. Calcular cuota mensual
                var cuotaMensual = _servicioCalculo.CalcularCuotaMensual(
                    datos.MontoCapital,
                    datos.TasaInteresAnual,
                    datos.PlazoMeses);

                // 5. Crear el préstamo
                var nuevoPrestamo = new Prestamo
                {
                    NumeroPrestamo = numeroPrestamo,
                    MontoCapital = datos.MontoCapital,
                    TasaInteresAnual = datos.TasaInteresAnual,
                    PlazoMeses = datos.PlazoMeses,
                    CuotaMensual = cuotaMensual,
                    EstaActivo = true,
                    ClienteId = datos.ClienteId,
                    AdministradorId = datos.AdministradorId,
                    FechaCreacion = DateTime.Now
                };

                await _repositorioPrestamo.AgregarAsync(nuevoPrestamo);
                await _repositorioPrestamo.GuardarCambiosAsync();

                // 6. Generar tabla de amortización
                var tablaAmortizacion = _servicioCalculo.GenerarTablaAmortizacion(
                    DateTime.Now,
                    datos.MontoCapital,
                    datos.TasaInteresAnual,
                    datos.PlazoMeses);

                foreach (var (fechaPago, montoCuota) in tablaAmortizacion)
                {
                    var cuota = new CuotaPrestamo
                    {
                        FechaPago = fechaPago,
                        MontoCuota = montoCuota,
                        EstaPagada = false,
                        EstaAtrasada = false,
                        PrestamoId = nuevoPrestamo.Id,
                        FechaCreacion = DateTime.Now
                    };

                    await _repositorioCuota.AgregarAsync(cuota);
                }

                await _repositorioCuota.GuardarCambiosAsync();

                // 7. Acreditar monto a cuenta principal
                var cuentaPrincipal = await _repositorioCuenta.ObtenerCuentaPrincipalAsync(datos.ClienteId);
                if (cuentaPrincipal != null)
                {
                    cuentaPrincipal.Balance += datos.MontoCapital;
                    await _repositorioCuenta.ActualizarAsync(cuentaPrincipal);

                    // Registrar transacción
                    var transaccion = new Transaccion
                    {
                        FechaTransaccion = DateTime.Now,
                        Monto = datos.MontoCapital,
                        TipoTransaccion = Constantes.TipoCredito,
                        Beneficiario = cuentaPrincipal.NumeroCuenta,
                        Origen = numeroPrestamo,
                        EstadoTransaccion = Constantes.EstadoAprobada,
                        CuentaAhorroId = cuentaPrincipal.Id,
                        FechaCreacion = DateTime.Now
                    };

                    await _repositorioTransaccion.AgregarAsync(transaccion);
                    await _repositorioTransaccion.GuardarCambiosAsync();
                }

                // 8. Enviar correo de notificación
                try
                {
                    await _servicioCorreo.EnviarNotificacionPrestamoAprobadoAsync(
                        cliente.Email,
                        cliente.NombreCompleto,
                        datos.MontoCapital,
                        datos.PlazoMeses,
                        datos.TasaInteresAnual,
                        cuotaMensual);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al enviar correo de préstamo aprobado");
                }

                _logger.LogInformation($"Préstamo {numeroPrestamo} asignado exitosamente a cliente {cliente.UserName}");

                // 9. Retornar DTO
                var prestamoDTO = _mapper.Map<PrestamoDTO>(nuevoPrestamo);
                return ResultadoOperacion<PrestamoDTO>.Ok(
                    prestamoDTO,
                    $"Préstamo asignado exitosamente. Número: {numeroPrestamo}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar préstamo");
                return ResultadoOperacion<PrestamoDTO>.Fallo("Ocurrió un error al procesar el préstamo");
            }
        }

        public async Task<ResultadoOperacion> ActualizarTasaInteresAsync(ActualizarTasaPrestamoDTO datos)
        {
            try
            {
                var prestamo = await _repositorioPrestamo.ObtenerPorIdAsync(datos.PrestamoId);
                if (prestamo == null)
                    return ResultadoOperacion.Fallo("Préstamo no encontrado");

                if (!prestamo.EstaActivo)
                    return ResultadoOperacion.Fallo("El préstamo no está activo");

                // Obtener cuotas futuras
                var cuotasFuturas = await _repositorioCuota.ObtenerCuotasFuturasAsync(datos.PrestamoId);

                if (cuotasFuturas.Any())
                {
                    int cuotasRestantes = cuotasFuturas.Count();
                    decimal capitalPendiente = cuotasFuturas.Sum(c => c.MontoCuota);

                    // Recalcular cuota con nueva tasa
                    var nuevaCuotaMensual = _servicioCalculo.CalcularCuotaMensual(
                        capitalPendiente,
                        datos.NuevaTasaInteres,
                        cuotasRestantes);

                    // Actualizar todas las cuotas futuras
                    foreach (var cuota in cuotasFuturas)
                    {
                        cuota.MontoCuota = nuevaCuotaMensual;
                        await _repositorioCuota.ActualizarAsync(cuota);
                    }

                    prestamo.CuotaMensual = nuevaCuotaMensual;
                }

                // Actualizar tasa del préstamo
                prestamo.TasaInteresAnual = datos.NuevaTasaInteres;
                await _repositorioPrestamo.ActualizarAsync(prestamo);
                await _repositorioPrestamo.GuardarCambiosAsync();

                // Enviar correo
                try
                {
                    var cliente = await _userManager.FindByIdAsync(prestamo.ClienteId);
                    await _servicioCorreo.EnviarNotificacionCambioTasaPrestamoAsync(
                        cliente.Email,
                        cliente.NombreCompleto,
                        datos.NuevaTasaInteres,
                        prestamo.CuotaMensual);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al enviar correo de cambio de tasa");
                }

                _logger.LogInformation($"Tasa de préstamo {prestamo.NumeroPrestamo} actualizada a {datos.NuevaTasaInteres}%");
                return ResultadoOperacion.Ok("Tasa de interés actualizada exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar tasa de interés");
                return ResultadoOperacion.Fallo("Error al actualizar la tasa de interés");
            }
        }

        public async Task<ResultadoOperacion<decimal>> ObtenerDeudaPromedioAsync()
        {
            try
            {
                var deudaPromedio = await _repositorioPrestamo.CalcularDeudaPromedioAsync();
                return ResultadoOperacion<decimal>.Ok(deudaPromedio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular deuda promedio");
                return ResultadoOperacion<decimal>.Fallo("Error al calcular la deuda promedio");
            }
        }

        public async Task<ResultadoOperacion<bool>> ValidarRiesgoClienteAsync(
            string clienteId,
            decimal montoNuevoPrestamo)
        {
            try
            {
                var deudaActual = await _repositorioPrestamo.CalcularDeudaTotalClienteAsync(clienteId);
                var deudaPromedio = await _repositorioPrestamo.CalcularDeudaPromedioAsync();

                // Calcular si es alto riesgo
                var esAltoRiesgo = deudaActual > deudaPromedio ||
                                  (deudaActual + montoNuevoPrestamo) > deudaPromedio;

                return ResultadoOperacion<bool>.Ok(esAltoRiesgo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar riesgo del cliente");
                return ResultadoOperacion<bool>.Fallo("Error al validar el riesgo");
            }
        }

        public async Task<ResultadoOperacion<PrestamoDTO>> ObtenerPrestamoPorIdAsync(int prestamoId)
        {
            try
            {
                var prestamo = await _repositorioPrestamo.ObtenerPorIdAsync(prestamoId);
                if (prestamo == null)
                    return ResultadoOperacion<PrestamoDTO>.Fallo("Préstamo no encontrado");

                var prestamoDTO = _mapper.Map<PrestamoDTO>(prestamo);
                return ResultadoOperacion<PrestamoDTO>.Ok(prestamoDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener préstamo");
                return ResultadoOperacion<PrestamoDTO>.Fallo("Error al obtener el préstamo");
            }
        }
    }
}
