using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.DTOs
{
    public class RetiroCajeroDTO
    {
        [Required]
        public string NumeroCuentaOrigen { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Monto { get; set; }

        [Required]
        public string CajeroId { get; set; }
    }
}