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
    public class ServicioTarjetaCredito : IServicioTarjetaCredito
    {
        private readonly IRepositorioTarjetaCredito _repositorioTarjeta;
        private readonly IServicioCifrado _servicioCifrado;
        private readonly IServicioCorreo _servicioCorreo;
        private readonly UserManager<Usuario> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<ServicioTarjetaCredito> _logger;

        public ServicioTarjetaCredito(
            IRepositorioTarjetaCredito repositorioTarjeta,
            IServicioCifrado servicioCifrado,
            IServicioCorreo servicioCorreo,
            UserManager<Usuario> userManager,
            IMapper mapper,
            ILogger<ServicioTarjetaCredito> logger)
        {
            _repositorioTarjeta = repositorioTarjeta;
            _servicioCifrado = servicioCifrado;
            _servicioCorreo = servicioCorreo;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Asigna una nueva tarjeta de crédito a un cliente
        /// Genera número único de 16 dígitos, CVC cifrado y fecha de expiración
        /// </summary>
        public async Task<ResultadoOperacion<TarjetaCreditoDTO>> AsignarTarjetaAsync(AsignarTarjetaDTO datos)
        {
            try
            {
                // 1. Validar que el cliente existe
                var cliente = await _userManager.FindByIdAsync(datos.ClienteId);
                if (cliente == null)
                {
                    return ResultadoOperacion<TarjetaCreditoDTO>.Fallo("Cliente no encontrado");
                }

                // 2. Generar número de tarjeta único (16 dígitos)
                var numeroTarjeta = await _repositorioTarjeta.GenerarNumeroTarjetaUnicoAsync();

                // 3. Generar CVC aleatorio de 3 dígitos y cifrarlo con SHA-256
                var random = new Random();
                var cvcPlano = random.Next(100, 1000).ToString(); // Genera número de 3 dígitos
                var cvcCifrado = _servicioCifrado.CifrarCVC(cvcPlano);

                // 4. Calcular fecha de expiración (3 años desde hoy, formato MM/AA)
                var fechaExpiracion = DateTime.Now.AddYears(3);
                var fechaExpiracionFormato = fechaExpiracion.ToString("MM/yy");

                // 5. Crear la nueva tarjeta
                var nuevaTarjeta = new TarjetaCredito
                {
                    NumeroTarjeta = numeroTarjeta,
                    LimiteCredito = datos.LimiteCredito,
                    DeudaActual = 0, // Empieza sin deuda
                    FechaExpiracion = fechaExpiracionFormato,
                    CVC = cvcCifrado, // CVC cifrado con SHA-256
                    EstaActiva = true,
                    ClienteId = datos.ClienteId,
                    AdministradorId = datos.AdministradorId,
                    FechaCreacion = DateTime.Now
                };

                await _repositorioTarjeta.AgregarAsync(nuevaTarjeta);
                await _repositorioTarjeta.GuardarCambiosAsync();

                _logger.LogInformation($"Tarjeta {numeroTarjeta} asignada a cliente {cliente.UserName}");

                // 6. Retornar DTO
                var tarjetaDTO = _mapper.Map<TarjetaCreditoDTO>(nuevaTarjeta);
                tarjetaDTO.NombreCliente = cliente.Nombre;
                tarjetaDTO.ApellidoCliente = cliente.Apellido;
                tarjetaDTO.CedulaCliente = cliente.Cedula;

                return ResultadoOperacion<TarjetaCreditoDTO>.Ok(
                    tarjetaDTO,
                    $"Tarjeta asignada exitosamente. Número: ****{nuevaTarjeta.UltimosCuatroDigitos}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar tarjeta");
                return ResultadoOperacion<TarjetaCreditoDTO>.Fallo("Error al asignar la tarjeta");
            }
        }

        /// <summary>
        /// Actualiza el límite de crédito de una tarjeta
        /// Valida que el nuevo límite no sea menor a la deuda actual
        /// Envía correo de notificación al cliente
        /// </summary>
        public async Task<ResultadoOperacion> ActualizarLimiteAsync(ActualizarLimiteTarjetaDTO datos)
        {
            try
            {
                // 1. Obtener la tarjeta
                var tarjeta = await _repositorioTarjeta.ObtenerPorIdAsync(datos.TarjetaId);

                if (tarjeta == null)
                {
                    return ResultadoOperacion.Fallo("Tarjeta no encontrada");
                }

                if (!tarjeta.EstaActiva)
                {
                    return ResultadoOperacion.Fallo("La tarjeta no está activa");
                }

                // 2. Validar que el nuevo límite no sea menor a la deuda actual
                if (datos.NuevoLimite < tarjeta.DeudaActual)
                {
                    return ResultadoOperacion.Fallo(
                        $"El nuevo límite (RD${datos.NuevoLimite:N2}) no puede ser menor a la deuda actual (RD${tarjeta.DeudaActual:N2})");
                }

                // 3. Actualizar el límite
                tarjeta.LimiteCredito = datos.NuevoLimite;
                await _repositorioTarjeta.ActualizarAsync(tarjeta);
                await _repositorioTarjeta.GuardarCambiosAsync();

                // 4. Enviar correo de notificación
                try
                {
                    tarjeta = await _repositorioTarjeta.ObtenerPorNumeroTarjetaAsync(tarjeta.NumeroTarjeta);

                    await _servicioCorreo.EnviarNotificacionCambioLimiteTarjetaAsync(
                        tarjeta.Cliente.Email,
                        tarjeta.Cliente.NombreCompleto,
                        tarjeta.UltimosCuatroDigitos,
                        datos.NuevoLimite);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al enviar correo de cambio de límite");
                }

                _logger.LogInformation($"Límite de tarjeta {tarjeta.NumeroTarjeta} actualizado a RD${datos.NuevoLimite}");

                return ResultadoOperacion.Ok("Límite de tarjeta actualizado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar límite de tarjeta");
                return ResultadoOperacion.Fallo("Error al actualizar el límite");
            }
        }

        /// <summary>
        /// Cancela una tarjeta de crédito
        /// Solo se puede cancelar si NO tiene deuda pendiente
        /// Una vez cancelada, no se puede usar para consumos o pagos
        /// </summary>
        public async Task<ResultadoOperacion> CancelarTarjetaAsync(int tarjetaId)
        {
            try
            {
                // 1. Obtener la tarjeta
                var tarjeta = await _repositorioTarjeta.ObtenerPorIdAsync(tarjetaId);

                if (tarjeta == null)
                {
                    return ResultadoOperacion.Fallo("Tarjeta no encontrada");
                }

                if (!tarjeta.EstaActiva)
                {
                    return ResultadoOperacion.Fallo("La tarjeta ya está cancelada");
                }

                // 2. Validar que NO tenga deuda pendiente
                if (tarjeta.DeudaActual > 0)
                {
                    return ResultadoOperacion.Fallo(
                        $"Para cancelar esta tarjeta, el cliente debe saldar la deuda pendiente de RD${tarjeta.DeudaActual:N2}");
                }

                // 3. Cancelar la tarjeta
                tarjeta.EstaActiva = false;
                await _repositorioTarjeta.ActualizarAsync(tarjeta);
                await _repositorioTarjeta.GuardarCambiosAsync();

                _logger.LogInformation($"Tarjeta {tarjeta.NumeroTarjeta} cancelada exitosamente");

                return ResultadoOperacion.Ok("Tarjeta cancelada exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar tarjeta");
                return ResultadoOperacion.Fallo("Error al cancelar la tarjeta");
            }
        }

        /// <summary>
        /// Obtiene una tarjeta por su ID con todas sus relaciones (cliente, consumos)
        /// </summary>
        public async Task<ResultadoOperacion<TarjetaCreditoDTO>> ObtenerTarjetaPorIdAsync(int tarjetaId)
        {
            try
            {
                var tarjeta = await _repositorioTarjeta.ObtenerPorIdAsync(tarjetaId);

                if (tarjeta == null)
                {
                    return ResultadoOperacion<TarjetaCreditoDTO>.Fallo("Tarjeta no encontrada");
                }

                var tarjetaDTO = _mapper.Map<TarjetaCreditoDTO>(tarjeta);
                return ResultadoOperacion<TarjetaCreditoDTO>.Ok(tarjetaDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tarjeta");
                return ResultadoOperacion<TarjetaCreditoDTO>.Fallo("Error al obtener la tarjeta");
            }
        }
    }
}
