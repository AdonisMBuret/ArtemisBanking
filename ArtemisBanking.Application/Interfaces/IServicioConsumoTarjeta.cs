using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.DTOs;

namespace ArtemisBanking.Application.Interfaces
{

    public interface IServicioConsumoTarjeta
    {

        Task<ResultadoOperacion<(string ultimosCuatroDigitos, string nombreCliente, decimal limiteCredito, decimal deudaActual, decimal creditoDisponible)>> 
            ObtenerInfoTarjetaAsync(string numeroTarjeta);

        Task<ResultadoOperacion> RegistrarConsumoAsync(RegistrarConsumoDTO datos);
    }
}
