using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Application.Interfaces.Services
{
    public interface IServicioCalculoPrestamo
    {
        // Calcular cuota mensual usando el sistema francés
        decimal CalcularCuotaMensual(decimal capital, decimal tasaInteresAnual, int plazoMeses);

        // Generar tabla de amortización completa
        List<(DateTime fechaPago, decimal montoCuota)> GenerarTablaAmortizacion(DateTime fechaInicio, decimal capital, decimal tasaInteresAnual, int plazoMeses);

        // Recalcular cuotas con nueva tasa de interés
        List<(int cuotaId, decimal nuevoMonto)> RecalcularCuotasConNuevaTasa(decimal capitalPendiente, decimal nuevaTasa, int cuotasRestantes);
    }
}
