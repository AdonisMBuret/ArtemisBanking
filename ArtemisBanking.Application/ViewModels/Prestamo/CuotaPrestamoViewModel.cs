using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Prestamo
{
    /// ViewModel para cada cuota en la tabla de amortización
    public class CuotaPrestamoViewModel
    {
        [Display(Name = "Fecha de Pago")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime FechaPago { get; set; }

        [Display(Name = "Monto de la Cuota")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal MontoCuota { get; set; }

        [Display(Name = "¿Pagada?")]
        public bool EstaPagada { get; set; }

        [Display(Name = "¿Atrasada?")]
        public bool EstaAtrasada { get; set; }
    }
}
