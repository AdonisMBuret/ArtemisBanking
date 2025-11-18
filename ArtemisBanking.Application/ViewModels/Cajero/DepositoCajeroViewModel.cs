using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Cajero
{
    /// <summary>
    /// ViewModel para realizar un depósito a una cuenta
    /// Incluye validaciones con mensajes amigables
    /// </summary>
    public class DepositoCajeroViewModel
    {
        [Required(ErrorMessage = "Necesitas el número de cuenta donde vas a depositar")]
        [RegularExpression(@"^[0-9]{9}$", ErrorMessage = "El número de cuenta debe tener 9 dígitos")]
        [Display(Name = "Número de Cuenta Destino")]
        public string NumeroCuentaDestino { get; set; }

        [Required(ErrorMessage = "No olvides el monto a depositar")]
        [Range(0.01, 1000000, ErrorMessage = "El monto debe estar entre RD$0.01 y RD$1,000,000")]
        [Display(Name = "Monto a Depositar")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal Monto { get; set; }
    }
}
