namespace ArtemisBanking.Application.DTOs
{
    /// <summary>
    /// DTO para registrar un consumo con tarjeta de crédito
    /// </summary>
    public class RegistrarConsumoDTO
    {
        public string NumeroTarjeta { get; set; } = string.Empty;
        public int ComercioId { get; set; }
        public decimal Monto { get; set; }
    }
}
