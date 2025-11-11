using ArtemisBanking.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Application.Interfaces
{
    public interface IServicioCuentaAhorro
    {
        /// <summary>
        /// Crea una cuenta de ahorro secundaria para un cliente
        /// Genera número único de 9 dígitos
        /// </summary>
        Task<ResultadoOperacion<CuentaAhorroDTO>> CrearCuentaSecundariaAsync(CrearCuentaSecundariaDTO datos);

        /// <summary>
        /// Cancela una cuenta secundaria
        /// Transfiere el balance a la cuenta principal si tiene fondos
        /// </summary>
        Task<ResultadoOperacion> CancelarCuentaAsync(int cuentaId, string usuarioId);

        /// <summary>
        /// Transfiere dinero entre cuentas propias del cliente
        /// Valida fondos y registra transacciones
        /// </summary>
        Task<ResultadoOperacion> TransferirEntreCuentasPropiasAsync(TransferirEntrePropiasDTO datos);

        /// <summary>
        /// Obtiene una cuenta por su ID con todas sus relaciones
        /// </summary>
        Task<ResultadoOperacion<CuentaAhorroDTO>> ObtenerCuentaPorIdAsync(int cuentaId);
    }
}
