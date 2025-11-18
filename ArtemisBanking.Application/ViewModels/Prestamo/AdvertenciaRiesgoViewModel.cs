using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Prestamo
{
    // ==================== ADVERTENCIA DE RIESGO ====================

    /// ViewModel para mostrar advertencia cuando el cliente es de alto riesgo
    public class AdvertenciaRiesgoViewModel
    {
        public string? ClienteId { get; set; }

        [Display(Name = "Cliente")]
        public string? NombreCliente { get; set; }

        [Display(Name = "Monto del Préstamo")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal MontoCapital { get; set; }

        [Display(Name = "Plazo")]
        public int PlazoMeses { get; set; }

        [Display(Name = "Tasa de Interés")]
        public decimal TasaInteresAnual { get; set; }

        [Display(Name = "Deuda Actual")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal DeudaActual { get; set; }

        [Display(Name = "Deuda Promedio del Sistema")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal DeudaPromedio { get; set; }

        [Display(Name = "Deuda Después del Préstamo")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal DeudaDespuesDelPrestamo { get; set; }

        public string? MensajeAdvertencia { get; set; }
    }
}