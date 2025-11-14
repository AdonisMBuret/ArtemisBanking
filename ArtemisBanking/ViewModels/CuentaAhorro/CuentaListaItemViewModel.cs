using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.ViewModels.CuentaAhorro
{
    public class CuentaListaItemViewModel
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
        public string TipoCuenta { get; set; } // "Principal" o "Secundaria"

        [Display(Name = "Estado")]
        public bool EstaActiva { get; set; }

        public bool EsPrincipal { get; set; }
    }
}
