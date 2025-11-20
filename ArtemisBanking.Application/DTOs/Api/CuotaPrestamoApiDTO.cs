
namespace ArtemisBanking.Application.DTOs.Api
{
    public class CuotaPrestamoApiDTO
    {
        public DateTime FechaPago { get; set; }
        public decimal MontoCuota { get; set; }
        public bool EstaPagada { get; set; }
        public bool EstaAtrasada { get; set; }
    }
}
