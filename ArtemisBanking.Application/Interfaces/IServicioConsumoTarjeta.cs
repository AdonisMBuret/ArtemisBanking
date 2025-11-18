using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.DTOs;

namespace ArtemisBanking.Application.Interfaces
{
    /// <summary>
    /// Servicio para gestionar consumos con tarjetas de crédito
    /// </summary>
    public interface IServicioConsumoTarjeta
    {
        /// <summary>
        /// Obtiene información de una tarjeta para validar antes de registrar consumo
        /// Retorna: (ultimosCuatroDigitos, nombreCliente, limiteCredito, deudaActual, creditoDisponible)
        /// </summary>
        Task<ResultadoOperacion<(string ultimosCuatroDigitos, string nombreCliente, decimal limiteCredito, decimal deudaActual, decimal creditoDisponible)>> 
            ObtenerInfoTarjetaAsync(string numeroTarjeta);

        /// <summary>
        /// Registra un consumo con tarjeta de crédito
        /// Valida que la tarjeta exista, esté activa y tenga crédito disponible
        /// </summary>
        Task<ResultadoOperacion> RegistrarConsumoAsync(RegistrarConsumoDTO datos);
    }
}
