using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.DTOs.Api
{
    public class ConfirmarCuentaRequestDTO
    {
        [Required(ErrorMessage = "El token es requerido")]
        public string Token { get; set; } = string.Empty;
    }
  
}
