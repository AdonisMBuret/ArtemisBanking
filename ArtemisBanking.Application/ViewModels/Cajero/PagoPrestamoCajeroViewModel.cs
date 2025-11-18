using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Cajero
{
    /// <summary>
    /// ViewModel para procesar un pago a préstamo
    /// Incluye validaciones con mensajes amigables
    /// </summary>
    public class PagoPrestamoCajeroViewModel
    {
        [Required(ErrorMessage = "Necesitas el número de cuenta de donde se tomará el dinero")]
        [RegularExpression(@"^[0-9]{9}$", ErrorMessage = "El número de cuenta debe tener 9 dígitos")]
        [Display(Name = "Número de Cuenta Origen")]
        public string NumeroCuentaOrigen { get; set; }

        [Required(ErrorMessage = "No olvides el número del préstamo")]
        [RegularExpression(@"^[0-9]{9}$", ErrorMessage = "El número de préstamo debe tener 9 dígitos")]
        [Display(Name = "Número de Préstamo")]
        public string NumeroPrestamo { get; set; }

        [Required(ErrorMessage = "Necesitas poner el monto que vas a pagar")]
        [Range(0.01, 1000000, ErrorMessage = "El monto debe estar entre RD$0.01 y RD$1,000,000")]
        [Display(Name = "Monto a Pagar")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal Monto { get; set; }
    }
}
