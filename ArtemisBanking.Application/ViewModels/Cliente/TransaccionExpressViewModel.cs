using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArtemisBanking.Application.ViewModels.Cliente
{
    /// <summary>
    /// ViewModel para transacciones express
    /// Incluye validaciones con mensajes amigables
    /// </summary>
    public class TransaccionExpressViewModel
    {
        [Required(ErrorMessage = "Debes seleccionar de qué cuenta vas a transferir")]
        [Display(Name = "Cuenta de Origen")]
        public int CuentaOrigenId { get; set; }

        [Required(ErrorMessage = "Necesitas el número de cuenta a donde vas a enviar")]
        [RegularExpression(@"^[0-9]{9}$", ErrorMessage = "El número de cuenta debe tener 9 dígitos")]
        [Display(Name = "Número de Cuenta Destino")]
        public string NumeroCuentaDestino { get; set; }

        [Required(ErrorMessage = "No olvides poner el monto a transferir")]
        [Range(0.01, 1000000, ErrorMessage = "El monto debe estar entre RD$0.01 y RD$1,000,000")]
        [Display(Name = "Monto a Transferir")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal Monto { get; set; }

        // Para el selector de cuentas
        public IEnumerable<SelectListItem> CuentasDisponibles { get; set; }
    }
}
