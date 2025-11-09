using ArtemisBanking.Application.Interfaces.Services;
using System;
using System.Collections.Generic;

namespace ArtemisBanking.Infrastructure.Services
{
    /// <summary>
    /// Servicio para cálculos del sistema francés de amortización
    /// </summary>
    public class ServicioCalculoPrestamo : IServicioCalculoPrestamo
    {
        /// <summary>
        /// Calcula la cuota mensual fija usando el sistema francés
        /// Fórmula: C = P * (r * (1 + r)^n) / ((1 + r)^n - 1)
        /// </summary>
        public decimal CalcularCuotaMensual(decimal capital, decimal tasaInteresAnual, int plazoMeses)
        {
            // Convertir tasa anual a mensual (dividir entre 12 y entre 100)
            var tasaMensual = (double)(tasaInteresAnual / 12 / 100);

            // Aplicar la fórmula del sistema francés
            var numerador = tasaMensual * Math.Pow(1 + tasaMensual, plazoMeses);
            var denominador = Math.Pow(1 + tasaMensual, plazoMeses) - 1;

            var cuota = (double)capital * (numerador / denominador);

            // Redondear a 2 decimales
            return Math.Round((decimal)cuota, 2);
        }

        /// <summary>
        /// Genera la tabla de amortización completa
        /// Retorna una lista de tuplas con fecha de pago y monto de cuota
        /// </summary>
        public List<(DateTime fechaPago, decimal montoCuota)> GenerarTablaAmortizacion(
            DateTime fechaInicio,
            decimal capital,
            decimal tasaInteresAnual,
            int plazoMeses)
        {
            var tabla = new List<(DateTime, decimal)>();
            var cuotaMensual = CalcularCuotaMensual(capital, tasaInteresAnual, plazoMeses);

            // Generar cada cuota
            for (int i = 1; i <= plazoMeses; i++)
            {
                // La primera cuota es un mes después de la fecha de inicio
                var fechaPago = fechaInicio.AddMonths(i);
                tabla.Add((fechaPago, cuotaMensual));
            }

            return tabla;
        }

        /// <summary>
        /// Recalcula las cuotas futuras cuando cambia la tasa de interés
        /// No implementado completamente (requiere cálculo más complejo del capital pendiente)
        /// </summary>
        public List<(int cuotaId, decimal nuevoMonto)> RecalcularCuotasConNuevaTasa(
            decimal capitalPendiente,
            decimal nuevaTasa,
            int cuotasRestantes)
        {
            var cuotasRecalculadas = new List<(int, decimal)>();
            var nuevaCuota = CalcularCuotaMensual(capitalPendiente, nuevaTasa, cuotasRestantes);

            // Aquí deberías recibir los IDs de las cuotas y asignarles el nuevo monto
            // Por ahora retornamos una lista vacía para implementar después
            return cuotasRecalculadas;
        }
    }
}