
namespace ArtemisBanking.Domain.Entities
{
    public class Prestamo : EntidadBase
    {
        public string NumeroPrestamo { get; set; }

        public decimal MontoCapital { get; set; }

        public decimal TasaInteresAnual { get; set; }

        public int PlazoMeses { get; set; }

        public decimal CuotaMensual { get; set; }

        public bool EstaActivo { get; set; } = true;

        public string ClienteId { get; set; }
        public virtual Usuario Cliente { get; set; }

        public string AdministradorId { get; set; }
        public virtual Usuario Administrador { get; set; }

        public virtual ICollection<CuotaPrestamo> TablaAmortizacion { get; set; }

        public Prestamo()
        {
            TablaAmortizacion = new List<CuotaPrestamo>();
        }

        public int CuotasPagadas => TablaAmortizacion?.Count(c => c.EstaPagada) ?? 0;

        public bool EstaAlDia => TablaAmortizacion?.All(c => !c.EstaAtrasada) ?? true;
    }
}

