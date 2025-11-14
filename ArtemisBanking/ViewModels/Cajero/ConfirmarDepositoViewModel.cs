using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.ViewModels.Cajero
{
    /// <summary>
    /// ViewModel para confirmar el depósito antes de procesarlo
    /// </summary>
    public class ConfirmarDepositoViewModel
    {
        [Display(Name = "Nombre del Cliente")]
        public string NombreCliente { get; set; }

        [Display(Name = "Apellido del Cliente")]
        public string ApellidoCliente { get; set; }

        [Display(Name = "Número de Cuenta")]
        public string NumeroCuentaDestino { get; set; }

        [Display(Name = "Monto")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Monto { get; set; }
    }
}
