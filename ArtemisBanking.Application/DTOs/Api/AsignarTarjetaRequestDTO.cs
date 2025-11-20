using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.DTOs.Api
{
    public class AsignarTarjetaRequestDTO
    {
        [Required(ErrorMessage = "El ID del cliente es requerido")]
        public string ClienteId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El límite de crédito es requerido")]
        [Range(1, double.MaxValue, ErrorMessage = "El límite debe ser mayor a 0")]
        public decimal LimiteCredito { get; set; }
    }
}
