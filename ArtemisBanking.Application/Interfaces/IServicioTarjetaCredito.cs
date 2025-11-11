using ArtemisBanking.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Application.Interfaces
{

    public interface IServicioTarjetaCredito
    {
        /// <summary>
        /// Asigna una nueva tarjeta de crédito a un cliente
        /// Genera número único, CVC cifrado y fecha de expiración
        /// </summary>
        Task<ResultadoOperacion<TarjetaCreditoDTO>> AsignarTarjetaAsync(AsignarTarjetaDTO datos);

        /// <summary>
        /// Actualiza el límite de crédito de una tarjeta
        /// Valida que no sea menor a la deuda actual
        /// </summary>
        Task<ResultadoOperacion> ActualizarLimiteAsync(ActualizarLimiteTarjetaDTO datos);

        /// <summary>
        /// Cancela una tarjeta de crédito
        /// Solo si no tiene deuda pendiente
        /// </summary>
        Task<ResultadoOperacion> CancelarTarjetaAsync(int tarjetaId);

        /// <summary>
        /// Obtiene una tarjeta por su ID con todas sus relaciones
        /// </summary>
        Task<ResultadoOperacion<TarjetaCreditoDTO>> ObtenerTarjetaPorIdAsync(int tarjetaId);
    }
}
