using ArtemisBanking.Application.DTOs;

namespace ArtemisBanking.Application.Interfaces
{
    public interface IServicioCuentaAhorro
    {
        /// Crea una cuenta de ahorro secundaria para un cliente
        /// Genera número único de 9 dígitos
        Task<ResultadoOperacion<CuentaAhorroDTO>> CrearCuentaSecundariaAsync(CrearCuentaSecundariaDTO datos);

        /// Cancela una cuenta secundaria
        /// Transfiere el balance a la cuenta principal si tiene fondos
        Task<ResultadoOperacion> CancelarCuentaAsync(int cuentaId);

        /// Transfiere dinero entre cuentas propias del cliente
        /// Valida fondos y registra transacciones
        Task<ResultadoOperacion> TransferirEntreCuentasPropiasAsync(TransferirEntrePropiasDTO datos);

        /// Obtiene una cuenta por su ID con todas sus relaciones
        Task<ResultadoOperacion<CuentaAhorroDTO>> ObtenerCuentaPorIdAsync(int cuentaId);

    }
}
