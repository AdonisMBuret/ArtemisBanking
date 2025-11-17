using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Cajero
{
    /// ViewModel para confirmar el pago a tarjeta antes de procesarlo
    public class ConfirmarPagoTarjetaViewModel
    {
        [Display(Name = "Nombre del Cliente")]
        public string NombreCliente { get; set; }

        [Display(Name = "Apellido del Cliente")]
        public string ApellidoCliente { get; set; }

        [Display(Name = "Número de Cuenta")]
        public string NumeroCuentaOrigen { get; set; }

        [Display(Name = "Últimos 4 Dígitos de la Tarjeta")]
        public string UltimosCuatroDigitosTarjeta { get; set; }

        [Display(Name = "Monto")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Monto { get; set; }
    }
}
