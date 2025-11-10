namespace ArtemisBanking.Application.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de cálculos de préstamos
    /// Define los métodos para calcular cuotas usando el sistema francés de amortización
    /// </summary>
    public interface IServicioCalculoPrestamo
    {
        // Calcular cuota mensual usando el sistema francés
        decimal CalcularCuotaMensual(decimal capital, decimal tasaInteresAnual, int plazoMeses);

        // Generar tabla de amortización completa
        List<(DateTime fechaPago, decimal montoCuota)> GenerarTablaAmortizacion(
            DateTime fechaInicio, 
            decimal capital, 
            decimal tasaInteresAnual, 
            int plazoMeses);

        // Recalcular cuotas con nueva tasa de interés
        List<(int cuotaId, decimal nuevoMonto)> RecalcularCuotasConNuevaTasa(
            decimal capitalPendiente, 
            decimal nuevaTasa, 
            int cuotasRestantes);
    }
}