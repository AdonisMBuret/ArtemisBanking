using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArtemisBanking.Web.ViewModels.Cliente
{
    /// <summary>
    /// ViewModel para pago a beneficiario
    /// </summary>
    public class PagoBeneficiarioViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un beneficiario")]
        [Display(Name = "Beneficiario")]
        public int BeneficiarioId { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto a Transferir")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una cuenta de origen")]
        [Display(Name = "Cuenta de Origen")]
        public int CuentaOrigenId { get; set; }

        public IEnumerable<SelectListItem> BeneficiariosDisponibles { get; set; }
        public IEnumerable<SelectListItem> CuentasDisponibles { get; set; }
    }
}