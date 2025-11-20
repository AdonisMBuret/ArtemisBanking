
namespace ArtemisBanking.Application.DTOs
{
    public class CuotaPrestamoDTO
    {
        public int Id { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal MontoCuota { get; set; }
        public bool EstaPagada { get; set; }
        public bool EstaAtrasada { get; set; }
    }
}