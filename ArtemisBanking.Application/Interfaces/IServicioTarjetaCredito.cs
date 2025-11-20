using ArtemisBanking.Application.DTOs;

namespace ArtemisBanking.Application.Interfaces
{

    public interface IServicioTarjetaCredito
    {
        Task<ResultadoOperacion<TarjetaCreditoDTO>> AsignarTarjetaAsync(AsignarTarjetaDTO datos);

        Task<ResultadoOperacion> ActualizarLimiteAsync(ActualizarLimiteTarjetaDTO datos);

        Task<ResultadoOperacion> CancelarTarjetaAsync(int tarjetaId);

        Task<ResultadoOperacion<TarjetaCreditoDTO>> ObtenerTarjetaPorIdAsync(int tarjetaId);
    }
}
