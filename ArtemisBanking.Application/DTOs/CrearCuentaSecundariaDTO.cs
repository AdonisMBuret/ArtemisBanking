using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.DTOs
{
    public class CrearCuentaSecundariaDTO
    {
        [Required]
        public string ClienteId { get; set; }

        [Required]
        public string AdministradorId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal BalanceInicial { get; set; } = 0;
    }

}
