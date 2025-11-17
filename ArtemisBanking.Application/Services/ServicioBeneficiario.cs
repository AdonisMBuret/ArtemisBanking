using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Domain.Interfaces.Repositories;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace ArtemisBanking.Application.Services
{
    public class ServicioBeneficiario : IServicioBeneficiario
    {
        private readonly IRepositorioBeneficiario _repositorioBeneficiario;
        private readonly IRepositorioCuentaAhorro _repositorioCuenta;
        private readonly IMapper _mapper;
        private readonly ILogger<ServicioBeneficiario> _logger;

        public ServicioBeneficiario(
            IRepositorioBeneficiario repositorioBeneficiario,
            IRepositorioCuentaAhorro repositorioCuenta,
            IMapper mapper,
            ILogger<ServicioBeneficiario> logger)
        {
            _repositorioBeneficiario = repositorioBeneficiario;
            _repositorioCuenta = repositorioCuenta;
            _mapper = mapper;
            _logger = logger;
        }

         
        /// Agrega un nuevo beneficiario para el usuario
        /// Primero valida que la cuenta existe y que no esté ya registrada como beneficiario
        public async Task<ResultadoOperacion> AgregarBeneficiarioAsync(string usuarioId, string numeroCuenta)
        {
            try
            {
                // 1. Validar que la cuenta destino existe y está activa
                var cuentaBeneficiario = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(numeroCuenta);

                if (cuentaBeneficiario == null || !cuentaBeneficiario.EstaActiva)
                {
                    return ResultadoOperacion.Fallo("El número de cuenta ingresado no es válido o está inactiva");
                }

                // 2. Validar que no se agregue su propia cuenta como beneficiario
                if (cuentaBeneficiario.UsuarioId == usuarioId)
                {
                    return ResultadoOperacion.Fallo("No puede agregar su propia cuenta como beneficiario");
                }

                // 3. Validar que no esté ya registrado como beneficiario
                var yaExiste = await _repositorioBeneficiario.ExisteBeneficiarioAsync(usuarioId, numeroCuenta);

                if (yaExiste)
                {
                    return ResultadoOperacion.Fallo("Esta cuenta ya está registrada como beneficiario");
                }

                // 4. Crear el nuevo beneficiario
                var nuevoBeneficiario = new Beneficiario
                {
                    NumeroCuentaBeneficiario = numeroCuenta,
                    UsuarioId = usuarioId,
                    CuentaAhorroId = cuentaBeneficiario.Id,
                    FechaCreacion = DateTime.Now
                };

                await _repositorioBeneficiario.AgregarAsync(nuevoBeneficiario);
                await _repositorioBeneficiario.GuardarCambiosAsync();

                _logger.LogInformation($"Beneficiario {numeroCuenta} agregado para usuario {usuarioId}");

                return ResultadoOperacion.Ok("Beneficiario agregado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar beneficiario");
                return ResultadoOperacion.Fallo("Error al agregar el beneficiario");
            }
        }

         
        /// Elimina un beneficiario del usuario
        /// Valida que el beneficiario pertenezca al usuario antes de eliminarlo
        public async Task<ResultadoOperacion> EliminarBeneficiarioAsync(int beneficiarioId, string usuarioId)
        {
            try
            {
                // 1. Obtener el beneficiario
                var beneficiario = await _repositorioBeneficiario.ObtenerPorIdAsync(beneficiarioId);

                if (beneficiario == null)
                {
                    return ResultadoOperacion.Fallo("Beneficiario no encontrado");
                }

                // 2. Validar que el beneficiario pertenezca al usuario
                if (beneficiario.UsuarioId != usuarioId)
                {
                    return ResultadoOperacion.Fallo("No tiene permiso para eliminar este beneficiario");
                }

                // 3. Eliminar el beneficiario
                await _repositorioBeneficiario.EliminarAsync(beneficiario);
                await _repositorioBeneficiario.GuardarCambiosAsync();

                _logger.LogInformation($"Beneficiario {beneficiarioId} eliminado para usuario {usuarioId}");

                return ResultadoOperacion.Ok("Beneficiario eliminado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar beneficiario");
                return ResultadoOperacion.Fallo("Error al eliminar el beneficiario");
            }
        }

         
        /// Obtiene todos los beneficiarios de un usuario
        /// Los retorna con la información de nombre y apellido del beneficiario
        public async Task<ResultadoOperacion<IEnumerable<BeneficiarioDTO>>> ObtenerBeneficiariosAsync(string usuarioId)
        {
            try
            {
                var beneficiarios = await _repositorioBeneficiario.ObtenerBeneficiariosDeUsuarioAsync(usuarioId);

                var beneficiariosDTO = _mapper.Map<IEnumerable<BeneficiarioDTO>>(beneficiarios);

                return ResultadoOperacion<IEnumerable<BeneficiarioDTO>>.Ok(beneficiariosDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener beneficiarios");
                return ResultadoOperacion<IEnumerable<BeneficiarioDTO>>.Fallo("Error al obtener los beneficiarios");
            }
        }
    }
}
