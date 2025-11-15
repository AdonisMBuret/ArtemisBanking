using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Cajero
{
    // ==================== PAGO A PRÉSTAMO ====================

    /// <summary>
    /// ViewModel para procesar un pago a préstamo
    /// </summary>
    public class PagoPrestamoCajeroViewModel
    {
        [Required(ErrorMessage = "El número de cuenta origen es obligatorio")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número de cuenta debe tener 9 dígitos")]
        [Display(Name = "Número de Cuenta Origen")]
        public string NumeroCuentaOrigen { get; set; }

        [Required(ErrorMessage = "El número de préstamo es obligatorio")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número de préstamo debe tener 9 dígitos")]
        [Display(Name = "Número de Préstamo")]
        public string NumeroPrestamo { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto a Pagar")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Monto { get; set; }
    }
}
