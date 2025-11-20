using ArtemisBanking.Application.Interfaces;

namespace ArtemisBanking.Application.Services
{
     
    /// Servicio para cálculos del sistema francés de amortización
     
    public class ServicioCalculoPrestamo : IServicioCalculoPrestamo
    {
         
        /// Calcula la cuota mensual fija usando el sistema francés
        /// Fórmula: C = P * (r * (1 + r)^n) / ((1 + r)^n - 1)
         
        public decimal CalcularCuotaMensual(decimal capital, decimal tasaInteresAnual, int plazoMeses)
        {
            var tasaMensual = (double)(tasaInteresAnual / 12 / 100);

            var numerador = tasaMensual * Math.Pow(1 + tasaMensual, plazoMeses);
            var denominador = Math.Pow(1 + tasaMensual, plazoMeses) - 1;

            var cuota = (double)capital * (numerador / denominador);

            return Math.Round((decimal)cuota, 2);
        }

         
        /// Genera la tabla de amortización completa
         
        public List<(DateTime fechaPago, decimal montoCuota)> GenerarTablaAmortizacion(
            DateTime fechaInicio,
            decimal capital,
            decimal tasaInteresAnual,
            int plazoMeses)
        {
            var tabla = new List<(DateTime, decimal)>();
            var cuotaMensual = CalcularCuotaMensual(capital, tasaInteresAnual, plazoMeses);

            for (int i = 1; i <= plazoMeses; i++)
            {
                var fechaPago = fechaInicio.AddMonths(i);
                tabla.Add((fechaPago, cuotaMensual));
            }

            return tabla;
        }

         
        /// Recalcula las cuotas futuras cuando cambia la tasa de interés
         
        public List<(int cuotaId, decimal nuevoMonto)> RecalcularCuotasConNuevaTasa(
            decimal capitalPendiente,
            decimal nuevaTasa,
            int cuotasRestantes)
        {
            var cuotasRecalculadas = new List<(int, decimal)>();
            var nuevaCuota = CalcularCuotaMensual(capitalPendiente, nuevaTasa, cuotasRestantes);

            return cuotasRecalculadas;
        }
    }
}