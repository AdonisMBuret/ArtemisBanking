
namespace ArtemisBanking.Application.Interfaces
{

    public interface IServicioCalculoPrestamo
    {
        decimal CalcularCuotaMensual(decimal capital, decimal tasaInteresAnual, int plazoMeses);

        List<(DateTime fechaPago, decimal montoCuota)> GenerarTablaAmortizacion(
            DateTime fechaInicio, 
            decimal capital, 
            decimal tasaInteresAnual, 
            int plazoMeses);

        List<(int cuotaId, decimal nuevoMonto)> RecalcularCuotasConNuevaTasa(
            decimal capitalPendiente, 
            decimal nuevaTasa, 
            int cuotasRestantes);
    }
}