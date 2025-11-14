using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.ViewModels.Prestamo
{
    // ==================== CONFIGURAR PRÉSTAMO ====================

    /// <summary>
    /// ViewModel para configurar los datos del préstamo a asignar
    /// </summary>
    public class ConfigurarPrestamoViewModel
    {
        [Required]
        public string ClienteId { get; set; }

        [Display(Name = "Cliente")]
        public string NombreCliente { get; set; }

        [Display(Name = "Deuda Actual del Cliente")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal DeudaActualCliente { get; set; }

        [Display(Name = "Deuda Promedio del Sistema")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal DeudaPromedio { get; set; }

        [Required(ErrorMessage = "El monto del capital es obligatorio")]
        [Range(100, double.MaxValue, ErrorMessage = "El monto debe ser mayor a RD$100")]
        [Display(Name = "Monto a Prestar")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal MontoCapital { get; set; }

        [Required(ErrorMessage = "Debe seleccionar el plazo del préstamo")]
        [Display(Name = "Plazo en Meses")]
        public int PlazoMeses { get; set; }

        [Required(ErrorMessage = "La tasa de interés es obligatoria")]
        [Range(0.01, 100, ErrorMessage = "La tasa debe estar entre 0.01% y 100%")]
        [Display(Name = "Tasa de Interés Anual (%)")]
        public decimal TasaInteresAnual { get; set; }
    }
}
