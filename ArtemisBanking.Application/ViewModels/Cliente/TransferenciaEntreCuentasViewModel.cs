using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArtemisBanking.Application.ViewModels.Cliente
{

    public class TransferenciaEntreCuentasViewModel
    {
        [Required(ErrorMessage = "Tienes que seleccionar una cuenta de origen")]
        [Display(Name = "Cuenta de Origen")]
        public int CuentaOrigenId { get; set; }

        [Required(ErrorMessage = "Necesitas elegir a qué cuenta vas a transferir")]
        [Display(Name = "Cuenta de Destino")]
        public int CuentaDestinoId { get; set; }

        [Required(ErrorMessage = "No te olvides del monto a transferir")]
        [Range(0.01, 1000000, ErrorMessage = "El monto debe estar entre RD$0.01 y RD$1,000,000")]
        [Display(Name = "Monto a Transferir")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal Monto { get; set; }

        public IEnumerable<SelectListItem>? CuentasDisponibles { get; set; }
    }

}