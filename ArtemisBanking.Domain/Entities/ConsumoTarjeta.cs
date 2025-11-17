
namespace ArtemisBanking.Domain.Entities
{
    public class ConsumoTarjeta : EntidadBase
    {
        // Fecha y hora del consumo
        public DateTime FechaConsumo { get; set; } = DateTime.Now;

        // Monto del consumo
        public decimal Monto { get; set; }

        // Nombre del comercio (o "AVANCE" si es avance de efectivo)
        public string NombreComercio { get; set; }

        // Estado del consumo (APROBADO o RECHAZADO)
        public string EstadoConsumo { get; set; }

        // Relación: Este consumo pertenece a una tarjeta
        public int TarjetaId { get; set; }
        public virtual TarjetaCredito Tarjeta { get; set; }
    }
}
