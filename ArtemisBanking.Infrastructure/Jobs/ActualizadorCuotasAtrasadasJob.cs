using ArtemisBanking.Domain.Interfaces;
using Hangfire;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ArtemisBanking.Infrastructure.Jobs
{
    /// <summary>
    /// Job de Hangfire que se ejecuta diariamente para actualizar cuotas atrasadas
    /// Revisa todas las cuotas y marca como atrasadas las que ya pasaron su fecha de pago
    /// </summary>
    public class ActualizadorCuotasAtrasadasJob
    {
        private readonly IRepositorioCuotaPrestamo _repositorioCuota;
        private readonly ILogger<ActualizadorCuotasAtrasadasJob> _logger;

        public ActualizadorCuotasAtrasadasJob(
            IRepositorioCuotaPrestamo repositorioCuota,
            ILogger<ActualizadorCuotasAtrasadasJob> logger)
        {
            _repositorioCuota = repositorioCuota;
            _logger = logger;
        }

        /// <summary>
        /// Ejecuta la actualización de cuotas atrasadas
        /// Este método se ejecuta automáticamente cada día a las 00:01
        /// </summary>
        [AutomaticRetry(Attempts = 3)]
        public async Task EjecutarAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando actualización de cuotas atrasadas...");

                await _repositorioCuota.ActualizarCuotasAtrasadasAsync();

                _logger.LogInformation("Actualización de cuotas atrasadas completada exitosamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cuotas atrasadas");
                throw;
            }
        }
    }
}