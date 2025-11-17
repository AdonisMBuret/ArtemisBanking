
namespace ArtemisBanking.Domain.Entities
{
    public class Prestamo : EntidadBase
    {
        // Número único de 9 dígitos que identifica el préstamo
        public string NumeroPrestamo { get; set; }

        // Monto total del préstamo (capital)
        public decimal MontoCapital { get; set; }

        // Tasa de interés anual (ejemplo: 12 para 12%)
        public decimal TasaInteresAnual { get; set; }

        // Duración del préstamo en meses (6, 12, 18, 24, 30, 36, 42, 48, 54, 60)
        public int PlazoMeses { get; set; }

        // Cuota mensual fija calculada con el sistema francés
        public decimal CuotaMensual { get; set; }

        // Estado del préstamo (activo o completado)
        public bool EstaActivo { get; set; } = true;

        // Relación: Este préstamo pertenece a un cliente
        public string ClienteId { get; set; }
        public virtual Usuario Cliente { get; set; }

        // Relación: El administrador que aprobó el préstamo
        public string AdministradorId { get; set; }
        public virtual Usuario Administrador { get; set; }

        // Relación: Un préstamo tiene muchas cuotas en la tabla de amortización
        public virtual ICollection<CuotaPrestamo> TablaAmortizacion { get; set; }

        public Prestamo()
        {
            TablaAmortizacion = new List<CuotaPrestamo>();
        }

        // Propiedad calculada: Total de cuotas pagadas
        public int CuotasPagadas => TablaAmortizacion?.Count(c => c.EstaPagada) ?? 0;

        // Propiedad calculada: Indica si el cliente está al día o en mora
        public bool EstaAlDia => TablaAmortizacion?.All(c => !c.EstaAtrasada) ?? true;
    }
}

