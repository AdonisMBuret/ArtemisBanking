using ArtemisBanking.Application.DTOs;

namespace ArtemisBanking.Application.Interfaces
{

    public interface IServicioDashboardCliente
    {

        Task<ResultadoOperacion<DashboardClienteDTO>> ObtenerDashboardAsync(string usuarioId);

        Task<ResultadoOperacion<DetalleCuentaClienteDTO>> ObtenerDetalleCuentaAsync(int cuentaId, string usuarioId);

        Task<ResultadoOperacion<DetallePrestamoClienteDTO>> ObtenerDetallePrestamoAsync(int prestamoId, string usuarioId);

        Task<ResultadoOperacion<DetalleTarjetaClienteDTO>> ObtenerDetalleTarjetaAsync(int tarjetaId, string usuarioId);
    }
}
