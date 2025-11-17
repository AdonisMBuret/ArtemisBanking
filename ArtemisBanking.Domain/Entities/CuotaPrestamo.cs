
namespace ArtemisBanking.Domain.Entities
{
    public class CuotaPrestamo : EntidadBase
    {
        // Fecha en que se debe pagar esta cuota
        public DateTime FechaPago { get; set; }

        // Monto de la cuota (calculado con sistema francés)
        public decimal MontoCuota { get; set; }

        // Indica si la cuota ya fue pagada
        public bool EstaPagada { get; set; } = false;

        // Indica si la cuota está atrasada (fecha pasó y no se pagó)
        public bool EstaAtrasada { get; set; } = false;

        // Relación: Esta cuota pertenece a un préstamo
        public int PrestamoId { get; set; }
        public virtual Prestamo Prestamo { get; set; }
    }
}
