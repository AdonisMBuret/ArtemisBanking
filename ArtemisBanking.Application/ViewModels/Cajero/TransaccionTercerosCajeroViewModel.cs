using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Cajero
{
    /// <summary>
    /// ViewModel para realizar una transacción entre dos cuentas de terceros
    /// Incluye validaciones con mensajes amigables
    /// </summary>
    public class TransaccionTercerosCajeroViewModel
    {
        [Required(ErrorMessage = "Necesitas el número de cuenta de donde se tomará el dinero")]
        [RegularExpression(@"^[0-9]{9}$", ErrorMessage = "El número de cuenta origen debe tener 9 dígitos")]
        [Display(Name = "Número de Cuenta Origen")]
        public string NumeroCuentaOrigen { get; set; }

        [Required(ErrorMessage = "Necesitas el número de cuenta a donde va el dinero")]
        [RegularExpression(@"^[0-9]{9}$", ErrorMessage = "El número de cuenta destino debe tener 9 dígitos")]
        [Display(Name = "Número de Cuenta Destino")]
        public string NumeroCuentaDestino { get; set; }

        [Required(ErrorMessage = "No olvides poner el monto a transferir")]
        [Range(0.01, 1000000, ErrorMessage = "El monto debe estar entre RD$0.01 y RD$1,000,000")]
        [Display(Name = "Monto a Transferir")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal Monto { get; set; }
    }
}
