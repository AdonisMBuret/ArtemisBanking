using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Cajero
{
    /// <summary>
    /// ViewModel para procesar un pago a tarjeta de crédito
    /// Incluye validaciones con mensajes amigables
    /// </summary>
    public class PagoTarjetaCajeroViewModel
    {
        [Required(ErrorMessage = "Necesitas el número de cuenta de donde se tomará el dinero")]
        [RegularExpression(@"^[0-9]{9}$", ErrorMessage = "El número de cuenta debe tener 9 dígitos")]
        [Display(Name = "Número de Cuenta Origen")]
        public string NumeroCuentaOrigen { get; set; }

        [Required(ErrorMessage = "No olvides el número de la tarjeta")]
        [RegularExpression(@"^[0-9]{16}$", ErrorMessage = "El número de tarjeta debe tener 16 dígitos")]
        [Display(Name = "Número de Tarjeta de Crédito")]
        public string NumeroTarjeta { get; set; }

        [Required(ErrorMessage = "Necesitas poner el monto que vas a pagar")]
        [Range(0.01, 1000000, ErrorMessage = "El monto debe estar entre RD$0.01 y RD$1,000,000")]
        [Display(Name = "Monto a Pagar")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal Monto { get; set; }
    }
}
