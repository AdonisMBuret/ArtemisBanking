using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.DTOs
{
    public class AsignarTarjetaDTO
    {
        [Required]
        public string ClienteId { get; set; }

        [Required]
        public string AdministradorId { get; set; }

        [Required]
        [Range(100, double.MaxValue)]
        public decimal LimiteCredito { get; set; }
    }
}
