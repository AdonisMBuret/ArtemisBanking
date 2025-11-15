using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Cliente
{
    public class AgregarBeneficiarioViewModel
    {
        [Required(ErrorMessage = "El número de cuenta es obligatorio")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número de cuenta debe tener 9 dígitos")]
        [Display(Name = "Número de Cuenta")]
        public string NumeroCuenta { get; set; }
    }
}
