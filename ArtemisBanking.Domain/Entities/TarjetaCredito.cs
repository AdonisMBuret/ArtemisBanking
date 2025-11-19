
namespace ArtemisBanking.Domain.Entities
{
    public class TarjetaCredito : EntidadBase
    {
        public string NumeroTarjeta { get; set; }

        public decimal LimiteCredito { get; set; }

        public decimal DeudaActual { get; set; } = 0;

        public string FechaExpiracion { get; set; }

        public string CVC { get; set; }

        public bool EstaActiva { get; set; } = true;

        public string ClienteId { get; set; }

        public virtual Usuario Cliente { get; set; }

        public string AdministradorId { get; set; }

        public virtual Usuario Administrador { get; set; }

        public virtual ICollection<ConsumoTarjeta> Consumos { get; set; }

        public TarjetaCredito()
        {
            Consumos = new List<ConsumoTarjeta>();
        }

        public decimal CreditoDisponible => LimiteCredito - DeudaActual;

        public string UltimosCuatroDigitos => NumeroTarjeta?.Length >= 4
            ? NumeroTarjeta.Substring(NumeroTarjeta.Length - 4)
            : NumeroTarjeta;
    }
}
