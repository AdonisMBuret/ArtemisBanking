using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.DTOs.Api
{

    public class CrearComercioRequestDTO
    {
        [Required(ErrorMessage = "El nombre del comercio es requerido")]
        [MaxLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El RNC es requerido")]
        [MaxLength(20, ErrorMessage = "El RNC no puede exceder 20 caracteres")]
        public string RNC { get; set; } = string.Empty;
    }
}
