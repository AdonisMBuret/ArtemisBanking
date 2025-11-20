
namespace ArtemisBanking.Application.DTOs.Api
{
    public class TransaccionApiDTO
    {
        public DateTime FechaTransaccion { get; set; }
        public decimal Monto { get; set; }
        public string TipoTransaccion { get; set; } = string.Empty;
        public string Beneficiario { get; set; } = string.Empty;
        public string Origen { get; set; } = string.Empty;
        public string EstadoTransaccion { get; set; } = string.Empty;
    }
}
