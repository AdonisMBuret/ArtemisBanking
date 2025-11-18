using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.CuentaAhorro
{
    public class CancelarCuentaViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Número de Cuenta")]
        public string? NumeroCuenta { get; set; }

        [Display(Name = "Cliente")]
        public string? NombreCliente { get; set; }

        [Display(Name = "Balance Actual")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Balance { get; set; }

        public bool EsPrincipal { get; set; }
    }
}
