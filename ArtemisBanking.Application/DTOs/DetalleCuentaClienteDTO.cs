
namespace ArtemisBanking.Application.DTOs
{
    public class DetalleCuentaClienteDTO
    {
        public int Id { get; set; }
        public string NumeroCuenta { get; set; }
        public decimal Balance { get; set; }
        public bool EsPrincipal { get; set; }
        public string TipoCuenta { get; set; }
        public IEnumerable<TransaccionDTO> Transacciones { get; set; }
    }
}
