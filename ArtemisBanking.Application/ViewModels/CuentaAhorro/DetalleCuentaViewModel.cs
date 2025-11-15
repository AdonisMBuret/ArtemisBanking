using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.CuentaAhorro
{
    public class DetalleCuentaViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Número de Cuenta")]
        public string NumeroCuenta { get; set; }

        [Display(Name = "Cliente")]
        public string NombreCliente { get; set; }

        [Display(Name = "Balance")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Balance { get; set; }

        [Display(Name = "Tipo")]
        public bool EsPrincipal { get; set; }

        [Display(Name = "Estado")]
        public bool EstaActiva { get; set; }

        public IEnumerable<TransaccionViewModel> Transacciones { get; set; }
    }
}
