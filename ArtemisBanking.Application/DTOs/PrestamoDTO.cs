
namespace ArtemisBanking.Application.DTOs
{
    public class PrestamoDTO
    {
        public int Id { get; set; }
        public string NumeroPrestamo { get; set; }
        public decimal MontoCapital { get; set; }
        public decimal TasaInteresAnual { get; set; }
        public int PlazoMeses { get; set; }
        public decimal CuotaMensual { get; set; }
        public bool EstaActivo { get; set; }
        public string NombreCliente { get; set; }
        public string ApellidoCliente { get; set; }
        public string CedulaCliente { get; set; }
        public int CuotasPagadas { get; set; }
        public int TotalCuotas { get; set; }
        public decimal MontoPendiente { get; set; }
        public bool EstaAlDia { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}