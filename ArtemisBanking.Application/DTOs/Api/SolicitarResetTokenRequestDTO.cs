using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.DTOs.Api
{
    public class SolicitarResetTokenRequestDTO
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        public string UserName { get; set; } = string.Empty;
    }

}
