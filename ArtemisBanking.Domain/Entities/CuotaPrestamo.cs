
namespace ArtemisBanking.Domain.Entities
{
    public class CuotaPrestamo : EntidadBase
    {
        public DateTime FechaPago { get; set; }

        public decimal MontoCuota { get; set; }

        public bool EstaPagada { get; set; } = false;

        public bool EstaAtrasada { get; set; } = false;

        public int PrestamoId { get; set; }
        public virtual Prestamo Prestamo { get; set; }
    }
}
