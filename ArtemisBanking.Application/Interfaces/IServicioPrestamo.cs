using ArtemisBanking.Application.DTOs;

namespace ArtemisBanking.Application.Interfaces
{
    public interface IServicioPrestamo
    {
         
        /// Asigna un nuevo préstamo a un cliente
        /// Valida riesgo, genera tabla de amortización y acredita fondos
         
        Task<ResultadoOperacion<PrestamoDTO>> AsignarPrestamoAsync(AsignarPrestamoDTO datos);

         
        /// Actualiza la tasa de interés de un préstamo
        /// Recalcula cuotas futuras y notifica al cliente
         
        Task<ResultadoOperacion> ActualizarTasaInteresAsync(ActualizarTasaPrestamoDTO datos);

         
        /// Obtiene la deuda promedio de todos los clientes
        /// Se usa para evaluar riesgo crediticio
         
        Task<ResultadoOperacion<decimal>> ObtenerDeudaPromedioAsync();

         
        /// Valida si un cliente es de alto riesgo
        /// Retorna true si la deuda supera el promedio
         
        Task<ResultadoOperacion<bool>> ValidarRiesgoClienteAsync(string clienteId, decimal montoNuevoPrestamo);

        /// Obtiene un préstamo por su ID con todas sus relaciones
        Task<ResultadoOperacion<PrestamoDTO>> ObtenerPrestamoPorIdAsync(int prestamoId);
    }
}
