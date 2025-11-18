using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Cliente
{
    /// <summary>
    /// ViewModel para agregar un beneficiario
    /// Incluye validaciones con mensajes amigables
    /// </summary>
    public class AgregarBeneficiarioViewModel
    {
        [Required(ErrorMessage = "Necesitas el número de cuenta del beneficiario")]
        [RegularExpression(@"^[0-9]{9}$", ErrorMessage = "El número de cuenta debe tener 9 dígitos")]
        [Display(Name = "Número de Cuenta")]
        public string NumeroCuenta { get; set; }
    }
}
