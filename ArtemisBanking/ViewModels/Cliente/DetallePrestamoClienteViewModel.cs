using ArtemisBanking.ViewModels.Prestamo;
using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.ViewModels.Cliente
{
    public class DetallePrestamoClienteViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Número de Préstamo")]
        public string NumeroPrestamo { get; set; }

        [Display(Name = "Monto del Capital")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal MontoCapital { get; set; }

        [Display(Name = "Tasa de Interés Anual")]
        [DisplayFormat(DataFormatString = "{0:P2}")]
        public decimal TasaInteresAnual { get; set; }

        public IEnumerable<CuotaPrestamoViewModel> TablaAmortizacion { get; set; }
    }
}
