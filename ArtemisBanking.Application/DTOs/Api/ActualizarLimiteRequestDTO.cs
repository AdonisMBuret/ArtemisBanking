using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.DTOs.Api
{
    public class ActualizarLimiteRequestDTO
    {
        [Required(ErrorMessage = "El nuevo límite es requerido")]
        [Range(1, double.MaxValue, ErrorMessage = "El límite debe ser mayor a 0")]
        public decimal NuevoLimite { get; set; }
    }
}
