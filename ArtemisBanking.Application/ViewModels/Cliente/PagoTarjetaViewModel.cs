using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArtemisBanking.Application.ViewModels.Cliente
{

    public class PagoTarjetaViewModel
    {
        [Required(ErrorMessage = "Debes elegir qué tarjeta vas a pagar")]
        [Display(Name = "Tarjeta de Crédito")]
        public int TarjetaId { get; set; }

        [Required(ErrorMessage = "Necesitas seleccionar de qué cuenta vas a pagar")]
        [Display(Name = "Cuenta de Origen")]
        public int CuentaOrigenId { get; set; }

        [Required(ErrorMessage = "No olvides poner el monto que vas a pagar")]
        [Range(0.01, 1000000, ErrorMessage = "El monto debe estar entre RD$0.01 y RD$1,000,000")]
        [Display(Name = "Monto a Pagar")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal Monto { get; set; }

        public IEnumerable<SelectListItem>? TarjetasDisponibles { get; set; }
        public IEnumerable<SelectListItem>? CuentasDisponibles { get; set; }
    }
}
