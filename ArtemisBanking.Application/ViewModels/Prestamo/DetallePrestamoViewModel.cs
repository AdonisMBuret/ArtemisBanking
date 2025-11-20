using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Prestamo
{
    public class DetallePrestamoViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Número de Préstamo")]
        public string NumeroPrestamo { get; set; }

        [Display(Name = "Cliente")]
        public string NombreCliente { get; set; }

        [Display(Name = "Monto del Capital")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal MontoCapital { get; set; }

        [Display(Name = "Tasa de Interés Anual")]
        [DisplayFormat(DataFormatString = "{0}%")]
        public decimal TasaInteresAnual { get; set; }

        [Display(Name = "Plazo en Meses")]
        public int PlazoMeses { get; set; }

        [Display(Name = "Cuota Mensual")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal CuotaMensual { get; set; }

        [Display(Name = "Estado")]
        public bool EstaActivo { get; set; }

        [Display(Name = "Fecha de Creación")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime FechaCreacion { get; set; }
        public IEnumerable<CuotaPrestamoViewModel> TablaAmortizacion { get; set; }
    }

}
