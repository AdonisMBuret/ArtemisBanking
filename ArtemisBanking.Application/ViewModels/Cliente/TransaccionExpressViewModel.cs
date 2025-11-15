using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ArtemisBanking.Application.ViewModels.Cliente
{
    public class TransaccionExpressViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar una cuenta de origen")]
        [Display(Name = "Cuenta de Origen")]
        public int CuentaOrigenId { get; set; }

        [Required(ErrorMessage = "El número de cuenta destino es obligatorio")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número de cuenta debe tener 9 dígitos")]
        [Display(Name = "Número de Cuenta Destino")]
        public string NumeroCuentaDestino { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto a Transferir")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Monto { get; set; }

        // Para el selector de cuentas
        public IEnumerable<SelectListItem> CuentasDisponibles { get; set; }
    }
}
