
namespace ArtemisBanking.Application.DTOs
{
    public class ConsumoTarjetaDTO
    {
        public int Id { get; set; }
        public DateTime FechaConsumo { get; set; }
        public decimal Monto { get; set; }
        public string NombreComercio { get; set; }
        public string EstadoConsumo { get; set; }
        public string NumeroTarjeta { get; set; }
    }
}

