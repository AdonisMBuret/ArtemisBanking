using ArtemisBanking.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Application.Interfaces
{
    public interface IServicioPrestamo
    {
        /// <summary>
        /// Asigna un nuevo préstamo a un cliente
        /// Valida riesgo, genera tabla de amortización y acredita fondos
        /// </summary>
        Task<ResultadoOperacion<PrestamoDTO>> AsignarPrestamoAsync(AsignarPrestamoDTO datos);

        /// <summary>
        /// Actualiza la tasa de interés de un préstamo
        /// Recalcula cuotas futuras y notifica al cliente
        /// </summary>
        Task<ResultadoOperacion> ActualizarTasaInteresAsync(ActualizarTasaPrestamoDTO datos);

        /// <summary>
        /// Obtiene la deuda promedio de todos los clientes
        /// Se usa para evaluar riesgo crediticio
        /// </summary>
        Task<ResultadoOperacion<decimal>> ObtenerDeudaPromedioAsync();

        /// <summary>
        /// Valida si un cliente es de alto riesgo
        /// Retorna true si la deuda supera el promedio
        /// </summary>
        Task<ResultadoOperacion<bool>> ValidarRiesgoClienteAsync(string clienteId, decimal montoNuevoPrestamo);

        /// <summary>
        /// Obtiene un préstamo por su ID con todas sus relaciones
        /// </summary>
        Task<ResultadoOperacion<PrestamoDTO>> ObtenerPrestamoPorIdAsync(int prestamoId);
    }
}
