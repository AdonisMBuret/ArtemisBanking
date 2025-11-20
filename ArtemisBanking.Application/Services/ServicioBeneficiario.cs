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

         
        public async Task<ResultadoOperacion> AgregarBeneficiarioAsync(string usuarioId, string numeroCuenta)
        {
            try
            {
                
                var cuentaBeneficiario = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(numeroCuenta);

                if (cuentaBeneficiario == null || !cuentaBeneficiario.EstaActiva)
                {
                    return ResultadoOperacion.Fallo("El número de cuenta ingresado no es válido o está inactiva");
                }

                if (cuentaBeneficiario.UsuarioId == usuarioId)
                {
                    return ResultadoOperacion.Fallo("No puede agregar su propia cuenta como beneficiario");
                }

                var yaExiste = await _repositorioBeneficiario.ExisteBeneficiarioAsync(usuarioId, numeroCuenta);

                if (yaExiste)
                {
                    return ResultadoOperacion.Fallo("Esta cuenta ya está registrada como beneficiario");
                }

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
        
        public async Task<ResultadoOperacion> EliminarBeneficiarioAsync(int beneficiarioId, string usuarioId)
        {
            try
            {
                var beneficiario = await _repositorioBeneficiario.ObtenerPorIdAsync(beneficiarioId);

                if (beneficiario == null)
                {
                    return ResultadoOperacion.Fallo("Beneficiario no encontrado");
                }

                if (beneficiario.UsuarioId != usuarioId)
                {
                    return ResultadoOperacion.Fallo("No tiene permiso para eliminar este beneficiario");
                }

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
