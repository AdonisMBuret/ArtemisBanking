using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Cajero
{
    /// ViewModel para confirmar el retiro antes de procesarlo
    public class ConfirmarRetiroViewModel
    {
        [Display(Name = "Nombre del Cliente")]
        public string NombreCliente { get; set; }

        [Display(Name = "Apellido del Cliente")]
        public string ApellidoCliente { get; set; }

        [Display(Name = "Número de Cuenta")]
        public string NumeroCuentaOrigen { get; set; }

        [Display(Name = "Monto")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Monto { get; set; }
    }
}
