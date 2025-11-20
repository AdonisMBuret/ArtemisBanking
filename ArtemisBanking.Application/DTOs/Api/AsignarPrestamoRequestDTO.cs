using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.DTOs.Api
{
    public class AsignarPrestamoRequestDTO
    {
        [Required(ErrorMessage = "El ID del cliente es requerido")]
        public string ClienteId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto del capital es requerido")]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal MontoCapital { get; set; }

        [Required(ErrorMessage = "El plazo en meses es requerido")]
        [Range(6, 60, ErrorMessage = "El plazo debe estar entre 6 y 60 meses")]
        public int PlazoMeses { get; set; }

        [Required(ErrorMessage = "La tasa de interés anual es requerida")]
        [Range(0.01, 99.99, ErrorMessage = "La tasa debe estar entre 0.01% y 99.99%")]
        public decimal TasaInteresAnual { get; set; }
    }
}
