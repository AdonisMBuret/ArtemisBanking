using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.ViewModels.Cliente
{
    /// <summary>
    /// ViewModel para confirmar la transacción antes de ejecutarla
    /// </summary>
    public class ConfirmarTransaccionViewModel
    {
        [Display(Name = "Nombre del Destinatario")]
        public string NombreDestinatario { get; set; }

        [Display(Name = "Apellido del Destinatario")]
        public string ApellidoDestinatario { get; set; }

        [Display(Name = "Cuenta Destino")]
        public string NumeroCuentaDestino { get; set; }

        [Display(Name = "Monto")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Monto { get; set; }

        public int CuentaOrigenId { get; set; }
    }
}
