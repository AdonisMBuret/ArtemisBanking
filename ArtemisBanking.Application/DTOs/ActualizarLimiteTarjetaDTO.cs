using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.DTOs
{
    public class ActualizarLimiteTarjetaDTO
    {
        [Required]
        public int TarjetaId { get; set; }

        [Required]
        [Range(100, double.MaxValue)]
        public decimal NuevoLimite { get; set; }
    }
}
