using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.ViewModels.Cliente
{
    public class PagoBeneficiarioViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un beneficiario")]
        [Display(Name = "Beneficiario")]
        public int BeneficiarioId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una cuenta de origen")]
        [Display(Name = "Cuenta de Origen")]
        public int CuentaOrigenId { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto a Transferir")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Monto { get; set; }

        // Para los selectores
        public IEnumerable<SelectListItem> BeneficiariosDisponibles { get; set; }
        public IEnumerable<SelectListItem> CuentasDisponibles { get; set; }
    }
}
