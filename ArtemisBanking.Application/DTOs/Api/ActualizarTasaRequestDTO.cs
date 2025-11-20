using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.DTOs.Api
{
    public class ActualizarTasaRequestDTO
    {
        [Required(ErrorMessage = "La nueva tasa de interés es requerida")]
        [Range(0.01, 99.99, ErrorMessage = "La tasa debe estar entre 0.01% y 99.99%")]
        public decimal NuevaTasaInteres { get; set; }
    }
}
