namespace ArtemisBanking.Application.DTOs.Api
{
    public class TarjetaCreditoApiResponseDTO
    {
        public int Id { get; set; }
        public string NumeroTarjeta { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public string ApellidoCliente { get; set; } = string.Empty;
        public string CedulaCliente { get; set; } = string.Empty;
        public decimal LimiteCredito { get; set; }
        public decimal DeudaActual { get; set; }
        public decimal CreditoDisponible { get; set; }
        public string FechaExpiracion { get; set; } = string.Empty;
        public bool EstaActiva { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
