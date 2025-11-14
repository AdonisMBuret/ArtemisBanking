using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.ViewModels.Cliente
{
    public class BeneficiarioItemViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Nombre")]
        public string NombreBeneficiario { get; set; }

        [Display(Name = "Apellido")]
        public string ApellidoBeneficiario { get; set; }

        [Display(Name = "Número de Cuenta")]
        public string NumeroCuentaBeneficiario { get; set; }
    }
}