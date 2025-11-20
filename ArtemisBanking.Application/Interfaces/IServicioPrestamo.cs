using ArtemisBanking.Application.DTOs;

namespace ArtemisBanking.Application.Interfaces
{
    public interface IServicioPrestamo
    {
                  
        Task<ResultadoOperacion<PrestamoDTO>> AsignarPrestamoAsync(AsignarPrestamoDTO datos);
                 
        Task<ResultadoOperacion> ActualizarTasaInteresAsync(ActualizarTasaPrestamoDTO datos);
                 
        Task<ResultadoOperacion<decimal>> ObtenerDeudaPromedioAsync();
                 
        Task<ResultadoOperacion<bool>> ValidarRiesgoClienteAsync(string clienteId, decimal montoNuevoPrestamo);

        Task<ResultadoOperacion<PrestamoDTO>> ObtenerPrestamoPorIdAsync(int prestamoId);
    }
}
