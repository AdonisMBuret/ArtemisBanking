using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArtemisBanking.Application.ViewModels.Cliente
{
    /// <summary>
    /// ViewModel para avance de efectivo desde tarjeta
    /// Incluye validaciones con mensajes amigables
    /// </summary>
    public class AvanceEfectivoViewModel
    {
        [Required(ErrorMessage = "Debes elegir de qué tarjeta vas a tomar el avance")]
        [Display(Name = "Tarjeta de Crédito Origen")]
        public int TarjetaId { get; set; }

        [Required(ErrorMessage = "Necesitas seleccionar a qué cuenta vas a depositar")]
        [Display(Name = "Cuenta de Ahorro Destino")]
        public int CuentaDestinoId { get; set; }

        [Required(ErrorMessage = "No olvides poner el monto del avance")]
        [Range(0.01, 1000000, ErrorMessage = "El monto debe estar entre RD$0.01 y RD$1,000,000")]
        [Display(Name = "Monto del Avance")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal Monto { get; set; }

        // Para los selectores
        public IEnumerable<SelectListItem> TarjetasDisponibles { get; set; }
        public IEnumerable<SelectListItem> CuentasDisponibles { get; set; }
    }
}
