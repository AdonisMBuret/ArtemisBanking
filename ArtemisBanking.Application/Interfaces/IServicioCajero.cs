using ArtemisBanking.Application.DTOs;

namespace ArtemisBanking.Application.Interfaces
{
    public interface IServicioCajero
    {
         
        Task<ResultadoOperacion> RealizarDepositoAsync(DepositoCajeroDTO datos);

        Task<ResultadoOperacion> RealizarRetiroAsync(RetiroCajeroDTO datos);

        Task<ResultadoOperacion> PagarTarjetaCreditoAsync(PagoTarjetaCajeroDTO datos);

        Task<ResultadoOperacion> PagarPrestamoAsync(PagoPrestamoCajeroDTO datos);

        Task<ResultadoOperacion> TransaccionEntreTercerosAsync(TransaccionTercerosCajeroDTO datos);

        Task<ResultadoOperacion<DashboardCajeroDTO>> ObtenerDashboardAsync(string cajeroId);
    }
}
