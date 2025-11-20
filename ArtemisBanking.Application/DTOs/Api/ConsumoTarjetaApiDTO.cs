
namespace ArtemisBanking.Application.DTOs.Api
{
    public class ConsumoTarjetaApiDTO
    {
        public DateTime FechaConsumo { get; set; }
        public decimal Monto { get; set; }
        public string NombreComercio { get; set; } = string.Empty;
        public string EstadoConsumo { get; set; } = string.Empty;
    }
}
