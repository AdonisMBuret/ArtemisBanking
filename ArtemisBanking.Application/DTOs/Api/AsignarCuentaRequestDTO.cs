using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.DTOs.Api
{
    public class AsignarCuentaRequestDTO
    {
        [Required(ErrorMessage = "El ID del cliente es requerido")]
        public string ClienteId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El balance inicial es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El balance debe ser mayor o igual a 0")]
        public decimal BalanceInicial { get; set; }
    }
}
