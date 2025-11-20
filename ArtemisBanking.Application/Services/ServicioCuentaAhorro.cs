using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Domain.Interfaces.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ArtemisBanking.Application.Services
{
    public class ServicioCuentaAhorro : IServicioCuentaAhorro
    {
        private readonly IRepositorioCuentaAhorro _repositorioCuenta;
        private readonly IRepositorioTransaccion _repositorioTransaccion;
        private readonly UserManager<Usuario> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<ServicioCuentaAhorro> _logger;

        public ServicioCuentaAhorro(
            IRepositorioCuentaAhorro repositorioCuenta,
            IRepositorioTransaccion repositorioTransaccion,
            UserManager<Usuario> userManager,
            IMapper mapper,
            ILogger<ServicioCuentaAhorro> logger)
        {
            _repositorioCuenta = repositorioCuenta;
            _repositorioTransaccion = repositorioTransaccion;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

                 
        public async Task<ResultadoOperacion<CuentaAhorroDTO>> CrearCuentaSecundariaAsync(CrearCuentaSecundariaDTO datos)
        {
            try
            {
                var cliente = await _userManager.FindByIdAsync(datos.ClienteId);
                if (cliente == null)
                {
                    return ResultadoOperacion<CuentaAhorroDTO>.Fallo("Cliente no encontrado");
                }

                var numeroCuenta = await _repositorioCuenta.GenerarNumeroCuentaUnicoAsync();

                var nuevaCuenta = new CuentaAhorro
                {
                    NumeroCuenta = numeroCuenta,
                    Balance = datos.BalanceInicial,
                    EsPrincipal = false, 
                    EstaActiva = true,
                    UsuarioId = datos.ClienteId,
                    FechaCreacion = DateTime.Now
                };

                await _repositorioCuenta.AgregarAsync(nuevaCuenta);
                await _repositorioCuenta.GuardarCambiosAsync();

                _logger.LogInformation($"Cuenta secundaria {numeroCuenta} creada para cliente {cliente.UserName}");

                var cuentaDTO = _mapper.Map<CuentaAhorroDTO>(nuevaCuenta);
                cuentaDTO.NombreCliente = cliente.Nombre;
                cuentaDTO.ApellidoCliente = cliente.Apellido;
                cuentaDTO.CedulaCliente = cliente.Cedula;

                return ResultadoOperacion<CuentaAhorroDTO>.Ok(
                    cuentaDTO,
                    $"Cuenta secundaria creada exitosamente. Número: {numeroCuenta}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cuenta secundaria");
                return ResultadoOperacion<CuentaAhorroDTO>.Fallo("Error al crear la cuenta secundaria");
            }
        }

         
        /// Cancela una cuenta secundaria
         public async Task<ResultadoOperacion> CancelarCuentaAsync(int cuentaId)
        {
            try
            {
                var cuenta = await _repositorioCuenta.ObtenerPorIdAsync(cuentaId);

                if (cuenta == null)
                {
                    return ResultadoOperacion.Fallo("Cuenta no encontrada");
                }

                 if (cuenta.EsPrincipal)
                {
                    return ResultadoOperacion.Fallo("No se puede cancelar la cuenta principal");
                }

                if (cuenta.Balance > 0)
                {
                    var cuentaPrincipal = await _repositorioCuenta.ObtenerCuentaPrincipalAsync(cuenta.UsuarioId);

                    if (cuentaPrincipal == null)
                    {
                        return ResultadoOperacion.Fallo("No se encontró la cuenta principal para transferir los fondos");
                    }

                    cuentaPrincipal.Balance += cuenta.Balance;
                    cuenta.Balance = 0;

                    await _repositorioCuenta.ActualizarAsync(cuentaPrincipal);

                    _logger.LogInformation(
                        $"Fondos transferidos de cuenta {cuenta.NumeroCuenta} a cuenta principal {cuentaPrincipal.NumeroCuenta}");
                }

                cuenta.EstaActiva = false;
                await _repositorioCuenta.ActualizarAsync(cuenta);
                await _repositorioCuenta.GuardarCambiosAsync();

                _logger.LogInformation($"Cuenta {cuenta.NumeroCuenta} cancelada exitosamente");

                return ResultadoOperacion.Ok("Cuenta cancelada exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar cuenta");
                return ResultadoOperacion.Fallo("Error al cancelar la cuenta");
            }
        }

         
        /// Transfiere dinero entre las cuentas propias del cliente

        public async Task<ResultadoOperacion> TransferirEntreCuentasPropiasAsync(TransferirEntrePropiasDTO datos)
        {
            try
            {
                var cuentaOrigen = await _repositorioCuenta.ObtenerPorIdAsync(datos.CuentaOrigenId);
                var cuentaDestino = await _repositorioCuenta.ObtenerPorIdAsync(datos.CuentaDestinoId);

                if (cuentaOrigen == null || cuentaDestino == null)
                {
                    return ResultadoOperacion.Fallo("Una o ambas cuentas no son válidas");
                }

                if (cuentaOrigen.UsuarioId != datos.UsuarioId || cuentaDestino.UsuarioId != datos.UsuarioId)
                {
                    return ResultadoOperacion.Fallo("Las cuentas deben pertenecer al mismo usuario");
                }

                if (datos.CuentaOrigenId == datos.CuentaDestinoId)
                {
                    return ResultadoOperacion.Fallo("No puede transferir a la misma cuenta");
                }

                if (!cuentaOrigen.EstaActiva || !cuentaDestino.EstaActiva)
                {
                    return ResultadoOperacion.Fallo("Una o ambas cuentas están inactivas");
                }

                if (cuentaOrigen.Balance < datos.Monto)
                {
                    return ResultadoOperacion.Fallo("Fondos insuficientes en la cuenta de origen");
                }

                cuentaOrigen.Balance -= datos.Monto;
                cuentaDestino.Balance += datos.Monto;

                await _repositorioCuenta.ActualizarAsync(cuentaOrigen);
                await _repositorioCuenta.ActualizarAsync(cuentaDestino);
                await _repositorioCuenta.GuardarCambiosAsync();

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

                _logger.LogInformation(
                    $"Transferencia exitosa: RD${datos.Monto} de {cuentaOrigen.NumeroCuenta} a {cuentaDestino.NumeroCuenta}");

                return ResultadoOperacion.Ok("Transferencia realizada exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al transferir entre cuentas propias");
                return ResultadoOperacion.Fallo("Error al realizar la transferencia");
            }
        }

         
        /// Obtiene una cuenta por su ID con todas sus relaciones (usuario, transacciones)
        public async Task<ResultadoOperacion<CuentaAhorroDTO>> ObtenerCuentaPorIdAsync(int cuentaId)
        {
            try
            {
                var cuenta = await _repositorioCuenta.ObtenerPorIdAsync(cuentaId);

                if (cuenta == null)
                {
                    return ResultadoOperacion<CuentaAhorroDTO>.Fallo("Cuenta no encontrada");
                }

                var cuentaDTO = _mapper.Map<CuentaAhorroDTO>(cuenta);
                return ResultadoOperacion<CuentaAhorroDTO>.Ok(cuentaDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cuenta");
                return ResultadoOperacion<CuentaAhorroDTO>.Fallo("Error al obtener la cuenta");
            }
        }
    }
}
