using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Web.ViewModels.Cliente
{
    /// <summary>
    /// ViewModel para la gestión de beneficiarios
    /// </summary>
    public class ListaBeneficiariosViewModel
    {
        public IEnumerable<BeneficiarioItemViewModel> Beneficiarios { get; set; }
    }

    public class BeneficiarioItemViewModel
    {
        public int Id { get; set; }
        public string NombreBeneficiario { get; set; }
        public string ApellidoBeneficiario { get; set; }
        public string NumeroCuentaBeneficiario { get; set; }
        public string NombreCompleto => $"{NombreBeneficiario} {ApellidoBeneficiario}";
    }

    /// <summary>
    /// ViewModel para agregar un nuevo beneficiario
    /// </summary>
    public class AgregarBeneficiarioViewModel
    {
        [Required(ErrorMessage = "El número de cuenta es requerido")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número de cuenta debe tener 9 dígitos")]
        [Display(Name = "Número de Cuenta")]
        public string NumeroCuenta { get; set; }
    }
}
