
namespace ArtemisBanking.Application.DTOs.Api
{
    public class TransaccionComercioDTO
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }
        public string TipoTransaccion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string? Origen { get; set; }
        public string? Beneficiario { get; set; }
    }
}
