namespace ArtemisBanking.Application.DTOs.Api
{
    public class PrestamoApiResponseDTO
    {
        public int Id { get; set; }
        public string NumeroPrestamo { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public string ApellidoCliente { get; set; } = string.Empty;
        public string CedulaCliente { get; set; } = string.Empty;
        public decimal MontoCapital { get; set; }
        public int TotalCuotas { get; set; }
        public int CuotasPagadas { get; set; }
        public decimal MontoPendiente { get; set; }
        public decimal TasaInteresAnual { get; set; }
        public int PlazoMeses { get; set; }
        public decimal CuotaMensual { get; set; }
        public bool EstaAlDia { get; set; }
        public bool EstaActivo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
