using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Web.ViewModels.Cajero
{
    /// <summary>
    /// ViewModel para pago a préstamo por cajero
    /// </summary>
    public class PagoPrestamoCajeroViewModel
    {
        [Required(ErrorMessage = "El número de cuenta origen es requerido")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número de cuenta debe tener 9 dígitos")]
        [Display(Name = "Número de Cuenta Origen")]
        public string NumeroCuentaOrigen { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto a Pagar")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "El número de préstamo es requerido")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número de préstamo debe tener 9 dígitos")]
        [Display(Name = "Número de Préstamo")]
        public string NumeroPrestamo { get; set; }
    }

    /// <summary>
    /// ViewModel para confirmar pago a préstamo
    /// </summary>
    public class ConfirmarPagoPrestamoCajeroViewModel
    {
        public string NombreTitular { get; set; }
        public string ApellidoTitular { get; set; }
        public string NumeroCuentaOrigen { get; set; }
        public string NumeroPrestamo { get; set; }
        public decimal Monto { get; set; }
        public decimal MontoPendientePrestamo { get; set; }
    }
}
