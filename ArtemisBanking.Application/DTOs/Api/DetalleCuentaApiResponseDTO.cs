namespace ArtemisBanking.Application.DTOs.Api
{
    public class DetalleCuentaApiResponseDTO
    {
        public int Id { get; set; }
        public string NumeroCuenta { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public string ApellidoCliente { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public bool EsPrincipal { get; set; }
        public bool EstaActiva { get; set; }
        public List<TransaccionApiDTO> Transacciones { get; set; } = new();
    }

}
