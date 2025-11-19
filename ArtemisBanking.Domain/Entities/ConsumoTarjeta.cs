namespace ArtemisBanking.Domain.Entities
{
    public class ConsumoTarjeta : EntidadBase
    {
        public DateTime FechaConsumo { get; set; } = DateTime.Now;

        public decimal Monto { get; set; }

        public string NombreComercio { get; set; }

        public string EstadoConsumo { get; set; }

        public int? ComercioId { get; set; }
        public virtual Comercio? Comercio { get; set; }

        public int TarjetaId { get; set; }
        public virtual TarjetaCredito Tarjeta { get; set; }
    }
}
