using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.DTOs
{
    public class TransaccionTercerosCajeroDTO
    {
        [Required]
        public string NumeroCuentaOrigen { get; set; }

        [Required]
        public string NumeroCuentaDestino { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Monto { get; set; }

        [Required]
        public string CajeroId { get; set; }
    }
}
