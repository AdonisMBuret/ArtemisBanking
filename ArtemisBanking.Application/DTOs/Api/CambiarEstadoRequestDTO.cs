using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.DTOs.Api
{
    public class CambiarEstadoRequestDTO
    {
        [Required(ErrorMessage = "El estado es requerido")]
        public bool Status { get; set; }
    }
}
