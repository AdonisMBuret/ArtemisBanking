using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace ArtemisBanking.Application.Services
{
    public class ServicioConsumoTarjeta : IServicioConsumoTarjeta
    {
        private readonly IRepositorioTarjetaCredito _repositorioTarjeta;
        private readonly IRepositorioConsumoTarjeta _repositorioConsumo;
        private readonly IRepositorioComercio _repositorioComercio;
        private readonly ILogger<ServicioConsumoTarjeta> _logger;

        public ServicioConsumoTarjeta(
            IRepositorioTarjetaCredito repositorioTarjeta,
            IRepositorioConsumoTarjeta repositorioConsumo,
            IRepositorioComercio repositorioComercio,
            ILogger<ServicioConsumoTarjeta> logger)
        {
            _repositorioTarjeta = repositorioTarjeta;
            _repositorioConsumo = repositorioConsumo;
            _repositorioComercio = repositorioComercio;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene información de una tarjeta para mostrar antes de confirmar el consumo
        /// </summary>
        public async Task<ResultadoOperacion<(string ultimosCuatroDigitos, string nombreCliente, decimal limiteCredito, decimal deudaActual, decimal creditoDisponible)>> 
            ObtenerInfoTarjetaAsync(string numeroTarjeta)
        {
            try
            {
                // Buscar la tarjeta
                var tarjeta = await _repositorioTarjeta.ObtenerPorNumeroTarjetaAsync(numeroTarjeta);

                if (tarjeta == null)
                {
                    return ResultadoOperacion<(string, string, decimal, decimal, decimal)>.Fallo(
                        "No se encontró ninguna tarjeta con ese número");
                }

                if (!tarjeta.EstaActiva)
                {
                    return ResultadoOperacion<(string, string, decimal, decimal, decimal)>.Fallo(
                        "Esta tarjeta no está activa");
                }

                // Retornar la información
                var info = (
                    tarjeta.UltimosCuatroDigitos,
                    $"{tarjeta.Cliente.Nombre} {tarjeta.Cliente.Apellido}",
                    tarjeta.LimiteCredito,
                    tarjeta.DeudaActual,
                    tarjeta.CreditoDisponible
                );

                return ResultadoOperacion<(string, string, decimal, decimal, decimal)>.Ok(info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener información de tarjeta");
                return ResultadoOperacion<(string, string, decimal, decimal, decimal)>.Fallo(
                    "Error al obtener la información de la tarjeta");
            }
        }

        /// <summary>
        /// Registra un consumo con tarjeta de crédito
        /// </summary>
        public async Task<ResultadoOperacion> RegistrarConsumoAsync(RegistrarConsumoDTO datos)
        {
            try
            {
                // 1. Validar que la tarjeta existe
                var tarjeta = await _repositorioTarjeta.ObtenerPorNumeroTarjetaAsync(datos.NumeroTarjeta);

                if (tarjeta == null)
                {
                    return ResultadoOperacion.Fallo("Tarjeta no encontrada");
                }

                if (!tarjeta.EstaActiva)
                {
                    return ResultadoOperacion.Fallo("La tarjeta no está activa");
                }

                // 2. Validar que hay crédito disponible
                if (tarjeta.CreditoDisponible < datos.Monto)
                {
                    return ResultadoOperacion.Fallo(
                        $"Crédito insuficiente. Disponible: RD${tarjeta.CreditoDisponible:N2}, Solicitado: RD${datos.Monto:N2}");
                }

                // 3. Validar que el comercio existe
                var comercio = await _repositorioComercio.ObtenerPorIdAsync(datos.ComercioId);

                if (comercio == null)
                {
                    return ResultadoOperacion.Fallo("Comercio no encontrado");
                }

                if (!comercio.EstaActivo)
                {
                    return ResultadoOperacion.Fallo("El comercio no está activo");
                }

                // 4. Actualizar la deuda de la tarjeta
                tarjeta.DeudaActual += datos.Monto;
                await _repositorioTarjeta.ActualizarAsync(tarjeta);

                // 5. Registrar el consumo
                var consumo = new ConsumoTarjeta
                {
                    FechaConsumo = DateTime.Now,
                    Monto = datos.Monto,
                    NombreComercio = comercio.Nombre,
                    EstadoConsumo = Constantes.ConsumoAprobado, // ? CORREGIDO
                    TarjetaId = tarjeta.Id,
                    ComercioId = comercio.Id,
                    FechaCreacion = DateTime.Now
                };

                await _repositorioConsumo.AgregarAsync(consumo);
                await _repositorioConsumo.GuardarCambiosAsync();

                _logger.LogInformation(
                    $"Consumo registrado: RD${datos.Monto} en {comercio.Nombre} con tarjeta {tarjeta.UltimosCuatroDigitos}");

                return ResultadoOperacion.Ok(
                    $"Consumo registrado exitosamente. Nueva deuda: RD${tarjeta.DeudaActual:N2}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar consumo");
                return ResultadoOperacion.Fallo("Error al procesar el consumo");
            }
        }
    }
}
