using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArtemisBanking.Application.ViewModels.Cliente
{
    /// <summary>
    /// ViewModel para realizar pagos a beneficiarios registrados
    /// Incluye validaciones completas y mensajes amigables
    /// </summary>
    public class PagoBeneficiarioViewModel
    {
        [Required(ErrorMessage = "Oye, tienes que elegir a quién le vas a pagar")]
        [Display(Name = "Beneficiario")]
        public int BeneficiarioId { get; set; }

        [Required(ErrorMessage = "Necesitas seleccionar de qué cuenta vas a pagar")]
        [Display(Name = "Cuenta de Origen")]
        public int CuentaOrigenId { get; set; }

        [Required(ErrorMessage = "No olvides poner el monto que vas a transferir")]
        [Range(0.01, 1000000, ErrorMessage = "El monto debe estar entre RD$0.01 y RD$1,000,000")]
        [Display(Name = "Monto a Transferir")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal Monto { get; set; }

        // Propiedades para los selectores en la vista
        public IEnumerable<SelectListItem> BeneficiariosDisponibles { get; set; }
        public IEnumerable<SelectListItem> CuentasDisponibles { get; set; }
    }
}
