
namespace ArtemisBanking.Application.DTOs
{
    public class DetalleTarjetaClienteDTO
    {
        public int Id { get; set; }
        public string NumeroTarjeta { get; set; }
        public decimal LimiteCredito { get; set; }
        public decimal DeudaActual { get; set; }
        public decimal CreditoDisponible { get; set; }
        public string FechaExpiracion { get; set; }
        public IEnumerable<ConsumoTarjetaDTO> Consumos { get; set; }
    }
}
