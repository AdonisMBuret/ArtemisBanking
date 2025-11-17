using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.DTOs
{
    public class PagoPrestamoClienteDTO
    {
        [Required]
        public int PrestamoId { get; set; }

        [Required]
        public int CuentaOrigenId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Monto { get; set; }

        [Required]
        public string UsuarioId { get; set; }
    }
}
