using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Cliente
{
    
    public class DetalleCuentaClienteViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Número de Cuenta")]
        public string NumeroCuenta { get; set; }

        [Display(Name = "Balance")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Balance { get; set; }

        [Display(Name = "Tipo de Cuenta")]
        public string TipoCuenta { get; set; }

        public bool EsPrincipal { get; set; }

        public IEnumerable<TransaccionClienteViewModel> Transacciones { get; set; }
    }
}
