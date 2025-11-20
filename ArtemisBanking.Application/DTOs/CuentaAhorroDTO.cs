
namespace ArtemisBanking.Application.DTOs
{
    public class CuentaAhorroDTO
    {
        public int Id { get; set; }
        public string NumeroCuenta { get; set; }
        public decimal Balance { get; set; }
        public bool EsPrincipal { get; set; }
        public bool EstaActiva { get; set; }
        public string NombreCliente { get; set; }
        public string ApellidoCliente { get; set; }
        public string CedulaCliente { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}