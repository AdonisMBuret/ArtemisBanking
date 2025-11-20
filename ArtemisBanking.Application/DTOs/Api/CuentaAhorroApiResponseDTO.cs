namespace ArtemisBanking.Application.DTOs.Api
{
    public class CuentaAhorroApiResponseDTO
    {
        public int Id { get; set; }
        public string NumeroCuenta { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public string ApellidoCliente { get; set; } = string.Empty;
        public string CedulaCliente { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public bool EsPrincipal { get; set; }
        public string TipoCuenta { get; set; } = string.Empty;
        public bool EstaActiva { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
