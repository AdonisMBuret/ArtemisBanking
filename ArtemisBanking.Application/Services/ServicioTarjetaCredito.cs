using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Domain.Interfaces.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

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

         
        /// Asigna una nueva tarjeta de crédito a un cliente

        public async Task<ResultadoOperacion<TarjetaCreditoDTO>> AsignarTarjetaAsync(AsignarTarjetaDTO datos)
        {
            try
            {
                var cliente = await _userManager.FindByIdAsync(datos.ClienteId);
                if (cliente == null)
                {
                    return ResultadoOperacion<TarjetaCreditoDTO>.Fallo("Cliente no encontrado");
                }

                var numeroTarjeta = await _repositorioTarjeta.GenerarNumeroTarjetaUnicoAsync();

                var random = new Random();
                var cvcPlano = random.Next(100, 1000).ToString(); // Genera número de 3 dígitos
                var cvcCifrado = _servicioCifrado.CifrarCVC(cvcPlano);

                var fechaExpiracion = DateTime.Now.AddYears(3);
                var fechaExpiracionFormato = fechaExpiracion.ToString("MM/yy");

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

         
        /// Actualiza el límite de crédito de una tarjeta

        public async Task<ResultadoOperacion> ActualizarLimiteAsync(ActualizarLimiteTarjetaDTO datos)
        {
            try
            {
                var tarjeta = await _repositorioTarjeta.ObtenerPorIdAsync(datos.TarjetaId);

                if (tarjeta == null)
                {
                    return ResultadoOperacion.Fallo("Tarjeta no encontrada");
                }

                if (!tarjeta.EstaActiva)
                {
                    return ResultadoOperacion.Fallo("La tarjeta no está activa");
                }

                if (datos.NuevoLimite < tarjeta.DeudaActual)
                {
                    return ResultadoOperacion.Fallo(
                        $"El nuevo límite (RD${datos.NuevoLimite:N2}) no puede ser menor a la deuda actual (RD${tarjeta.DeudaActual:N2})");
                }

                tarjeta.LimiteCredito = datos.NuevoLimite;
                await _repositorioTarjeta.ActualizarAsync(tarjeta);
                await _repositorioTarjeta.GuardarCambiosAsync();

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

         
        /// Cancela una tarjeta de crédito

        public async Task<ResultadoOperacion> CancelarTarjetaAsync(int tarjetaId)
        {
            try
            {
                var tarjeta = await _repositorioTarjeta.ObtenerPorIdAsync(tarjetaId);

                if (tarjeta == null)
                {
                    return ResultadoOperacion.Fallo("Tarjeta no encontrada");
                }

                if (!tarjeta.EstaActiva)
                {
                    return ResultadoOperacion.Fallo("La tarjeta ya está cancelada");
                }

                if (tarjeta.DeudaActual > 0)
                {
                    return ResultadoOperacion.Fallo(
                        $"Para cancelar esta tarjeta, el cliente debe saldar la deuda pendiente de RD${tarjeta.DeudaActual:N2}");
                }

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

         
        /// Obtiene una tarjeta por su ID con todas sus relaciones (cliente, consumos)
         
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