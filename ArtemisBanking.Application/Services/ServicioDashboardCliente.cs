using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Domain.Interfaces.Repositories;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace ArtemisBanking.Application.Services
{

    public class ServicioDashboardCliente : IServicioDashboardCliente
    {
        private readonly IRepositorioCuentaAhorro _repositorioCuenta;
        private readonly IRepositorioPrestamo _repositorioPrestamo;
        private readonly IRepositorioCuotaPrestamo _repositorioCuotaPrestamo;
        private readonly IRepositorioTarjetaCredito _repositorioTarjeta;
        private readonly IRepositorioConsumoTarjeta _repositorioConsumoTarjeta;
        private readonly IRepositorioTransaccion _repositorioTransaccion;
        private readonly IMapper _mapper;
        private readonly ILogger<ServicioDashboardCliente> _logger;

        public ServicioDashboardCliente(
            IRepositorioCuentaAhorro repositorioCuenta,
            IRepositorioPrestamo repositorioPrestamo,
            IRepositorioCuotaPrestamo repositorioCuotaPrestamo,
            IRepositorioTarjetaCredito repositorioTarjeta,
            IRepositorioConsumoTarjeta repositorioConsumoTarjeta,
            IRepositorioTransaccion repositorioTransaccion,
            IMapper mapper,
            ILogger<ServicioDashboardCliente> logger)
        {
            _repositorioCuenta = repositorioCuenta;
            _repositorioPrestamo = repositorioPrestamo;
            _repositorioCuotaPrestamo = repositorioCuotaPrestamo;
            _repositorioTarjeta = repositorioTarjeta;
            _repositorioConsumoTarjeta = repositorioConsumoTarjeta;
            _repositorioTransaccion = repositorioTransaccion;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResultadoOperacion<DashboardClienteDTO>> ObtenerDashboardAsync(string usuarioId)
        {
            try
            {
                var cuentas = await _repositorioCuenta.ObtenerCuentasActivasDeUsuarioAsync(usuarioId);
                var cuentasDTO = _mapper.Map<IEnumerable<CuentaAhorroDTO>>(cuentas);

                var prestamos = await _repositorioPrestamo.ObtenerPrestamosDeUsuarioAsync(usuarioId);
                var prestamosActivos = prestamos.Where(p => p.EstaActivo);
                var prestamosDTO = _mapper.Map<IEnumerable<PrestamoDTO>>(prestamosActivos);

                var tarjetas = await _repositorioTarjeta.ObtenerTarjetasActivasDeUsuarioAsync(usuarioId);
                var tarjetasDTO = _mapper.Map<IEnumerable<TarjetaCreditoDTO>>(tarjetas);

                var dashboard = new DashboardClienteDTO
                {
                    CuentasAhorro = cuentasDTO,
                    Prestamos = prestamosDTO,
                    TarjetasCredito = tarjetasDTO
                };

                return ResultadoOperacion<DashboardClienteDTO>.Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener dashboard del cliente {usuarioId}");
                return ResultadoOperacion<DashboardClienteDTO>.Fallo(
                    "Ups, no pudimos cargar tus productos. Intenta de nuevo");
            }
        }

        public async Task<ResultadoOperacion<DetalleCuentaClienteDTO>> ObtenerDetalleCuentaAsync(
            int cuentaId, 
            string usuarioId)
        {
            try
            {
                var cuenta = await _repositorioCuenta.ObtenerPorIdAsync(cuentaId);

                if (cuenta == null || cuenta.UsuarioId != usuarioId)
                {
                    return ResultadoOperacion<DetalleCuentaClienteDTO>.Fallo(
                        "Esa cuenta no existe o no es tuya");
                }

                var transacciones = await _repositorioTransaccion.ObtenerTransaccionesDeCuentaAsync(cuentaId);

                var detalle = new DetalleCuentaClienteDTO
                {
                    Id = cuenta.Id,
                    NumeroCuenta = cuenta.NumeroCuenta,
                    Balance = cuenta.Balance,
                    EsPrincipal = cuenta.EsPrincipal,
                    TipoCuenta = cuenta.EsPrincipal ? "Principal" : "Secundaria",
                    Transacciones = _mapper.Map<IEnumerable<TransaccionDTO>>(transacciones)
                };

                return ResultadoOperacion<DetalleCuentaClienteDTO>.Ok(detalle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener detalle de cuenta {cuentaId}");
                return ResultadoOperacion<DetalleCuentaClienteDTO>.Fallo(
                    "No pudimos cargar el detalle de la cuenta. Intenta de nuevo");
            }
        }

        public async Task<ResultadoOperacion<DetallePrestamoClienteDTO>> ObtenerDetallePrestamoAsync(
            int prestamoId, 
            string usuarioId)
        {
            try
            {
                var prestamo = await _repositorioPrestamo.ObtenerPorIdAsync(prestamoId);

                if (prestamo == null || prestamo.ClienteId != usuarioId)
                {
                    return ResultadoOperacion<DetallePrestamoClienteDTO>.Fallo(
                        "Ese préstamo no existe o no es tuyo");
                }

                var cuotas = await _repositorioCuotaPrestamo.ObtenerCuotasDePrestamoAsync(prestamoId);

                // Mapear a DTO
                var detalle = new DetallePrestamoClienteDTO
                {
                    Id = prestamo.Id,
                    NumeroPrestamo = prestamo.NumeroPrestamo,
                    MontoCapital = prestamo.MontoCapital,
                    TasaInteresAnual = prestamo.TasaInteresAnual,
                    TablaAmortizacion = _mapper.Map<IEnumerable<CuotaPrestamoDTO>>(cuotas)
                };

                return ResultadoOperacion<DetallePrestamoClienteDTO>.Ok(detalle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener detalle de préstamo {prestamoId}");
                return ResultadoOperacion<DetallePrestamoClienteDTO>.Fallo(
                    "No pudimos cargar el detalle del préstamo. Intenta de nuevo");
            }
        }

        public async Task<ResultadoOperacion<DetalleTarjetaClienteDTO>> ObtenerDetalleTarjetaAsync(
            int tarjetaId, 
            string usuarioId)
        {
            try
            {
                var tarjeta = await _repositorioTarjeta.ObtenerPorIdAsync(tarjetaId);

                if (tarjeta == null || tarjeta.ClienteId != usuarioId)
                {
                    return ResultadoOperacion<DetalleTarjetaClienteDTO>.Fallo(
                        "Esa tarjeta no existe o no es tuya");
                }

                var consumos = await _repositorioConsumoTarjeta.ObtenerConsumosDeTarjetaAsync(tarjetaId);

                var detalle = new DetalleTarjetaClienteDTO
                {
                    Id = tarjeta.Id,
                    NumeroTarjeta = tarjeta.NumeroTarjeta,
                    LimiteCredito = tarjeta.LimiteCredito,
                    DeudaActual = tarjeta.DeudaActual,
                    CreditoDisponible = tarjeta.CreditoDisponible,
                    FechaExpiracion = tarjeta.FechaExpiracion,
                    Consumos = _mapper.Map<IEnumerable<ConsumoTarjetaDTO>>(consumos)
                };

                return ResultadoOperacion<DetalleTarjetaClienteDTO>.Ok(detalle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener detalle de tarjeta {tarjetaId}");
                return ResultadoOperacion<DetalleTarjetaClienteDTO>.Fallo(
                    "No pudimos cargar el detalle de la tarjeta. Intenta de nuevo");
            }
        }
    }
}
