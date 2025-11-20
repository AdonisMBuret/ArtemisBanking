using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Prestamo
{

    public class PrestamoListaItemViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Número de Préstamo")]
        public string NumeroPrestamo { get; set; }

        [Display(Name = "Cliente")]
        public string NombreCliente { get; set; }

        [Display(Name = "Monto del Capital")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal MontoCapital { get; set; }

        [Display(Name = "Total de Cuotas")]
        public int TotalCuotas { get; set; }

        [Display(Name = "Cuotas Pagadas")]
        public int CuotasPagadas { get; set; }

        [Display(Name = "Monto Pendiente")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal MontoPendiente { get; set; }

        [Display(Name = "Tasa de Interés")]
        [DisplayFormat(DataFormatString = "{0}%")]
        public decimal TasaInteresAnual { get; set; }

        [Display(Name = "Plazo (meses)")]
        public int PlazoMeses { get; set; }

        [Display(Name = "Estado")]
        public bool EstaAlDia { get; set; }
    }
}
