using ArtemisBanking.Domain.Interfaces.Repositories;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace ArtemisBanking.Infrastructure.Jobs
{
    ///Job de Hangfire es para que se ejecute diariamente para actualizar cuotas atrasadas
    
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