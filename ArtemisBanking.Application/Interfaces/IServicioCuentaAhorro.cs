using ArtemisBanking.Application.DTOs;

namespace ArtemisBanking.Application.Interfaces
{
    public interface IServicioCuentaAhorro
    {

        Task<ResultadoOperacion<CuentaAhorroDTO>> CrearCuentaSecundariaAsync(CrearCuentaSecundariaDTO datos);

        Task<ResultadoOperacion> CancelarCuentaAsync(int cuentaId);

        Task<ResultadoOperacion> TransferirEntreCuentasPropiasAsync(TransferirEntrePropiasDTO datos);

        Task<ResultadoOperacion<CuentaAhorroDTO>> ObtenerCuentaPorIdAsync(int cuentaId);

    }
}
