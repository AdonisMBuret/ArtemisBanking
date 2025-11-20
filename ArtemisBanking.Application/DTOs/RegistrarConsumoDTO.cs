namespace ArtemisBanking.Application.DTOs
{
    public class RegistrarConsumoDTO
    {
        public string NumeroTarjeta { get; set; } = string.Empty;
        public int ComercioId { get; set; }
        public decimal Monto { get; set; }
    }
}