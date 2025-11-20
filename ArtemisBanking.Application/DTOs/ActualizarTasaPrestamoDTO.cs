using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.DTOs
{
    public class ActualizarTasaPrestamoDTO
    {
        [Required]
        public int PrestamoId { get; set; }

        [Required]
        [Range(0.01, 100)]
        public decimal NuevaTasaInteres { get; set; }
    }
}
