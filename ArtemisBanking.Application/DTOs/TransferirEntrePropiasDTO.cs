using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.DTOs
{
    public class TransferirEntrePropiasDTO
    {
        [Required]
        public int CuentaOrigenId { get; set; }

        [Required]
        public int CuentaDestinoId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Monto { get; set; }

        [Required]
        public string UsuarioId { get; set; }
    }
}
