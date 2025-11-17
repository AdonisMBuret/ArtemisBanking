using ArtemisBanking.Application.DTOs;

namespace ArtemisBanking.Application.Interfaces
{

    public interface IServicioTarjetaCredito
    {
        /// Asigna una nueva tarjeta de crédito a un cliente
        /// Genera número único, CVC cifrado y fecha de expiración
        Task<ResultadoOperacion<TarjetaCreditoDTO>> AsignarTarjetaAsync(AsignarTarjetaDTO datos);

        /// Actualiza el límite de crédito de una tarjeta
        /// Valida que no sea menor a la deuda actual
        Task<ResultadoOperacion> ActualizarLimiteAsync(ActualizarLimiteTarjetaDTO datos);

        /// Cancela una tarjeta de crédito
        /// Solo si no tiene deuda pendiente
        Task<ResultadoOperacion> CancelarTarjetaAsync(int tarjetaId);

        /// Obtiene una tarjeta por su ID con todas sus relaciones
        Task<ResultadoOperacion<TarjetaCreditoDTO>> ObtenerTarjetaPorIdAsync(int tarjetaId);
    }
}
