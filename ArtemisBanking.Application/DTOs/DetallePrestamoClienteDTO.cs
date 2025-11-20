
namespace ArtemisBanking.Application.DTOs
{
    public class DetallePrestamoClienteDTO
    {
        public int Id { get; set; }
        public string NumeroPrestamo { get; set; }
        public decimal MontoCapital { get; set; }
        public decimal TasaInteresAnual { get; set; }
        public IEnumerable<CuotaPrestamoDTO> TablaAmortizacion { get; set; }
    }
}
