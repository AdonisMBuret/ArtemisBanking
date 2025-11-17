
namespace ArtemisBanking.Application.DTOs
{
    public class TransaccionDTO
    {
        public int Id { get; set; }
        public DateTime FechaTransaccion { get; set; }
        public decimal Monto { get; set; }
        public string TipoTransaccion { get; set; }
        public string Beneficiario { get; set; }
        public string Origen { get; set; }
        public string EstadoTransaccion { get; set; }
        public string NumeroCuenta { get; set; }
    }
}
