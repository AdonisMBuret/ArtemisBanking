using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.ViewModels.Cajero
{
    /// <summary>
    /// ViewModel para confirmar la transacción entre terceros antes de procesarla
    /// </summary>
    public class ConfirmarTransaccionTercerosViewModel
    {
        [Display(Name = "Cliente Origen")]
        public string NombreClienteOrigen { get; set; }

        [Display(Name = "Apellido Cliente Origen")]
        public string ApellidoClienteOrigen { get; set; }

        [Display(Name = "Cuenta Origen")]
        public string NumeroCuentaOrigen { get; set; }

        [Display(Name = "Cliente Destino")]
        public string NombreClienteDestino { get; set; }

        [Display(Name = "Apellido Cliente Destino")]
        public string ApellidoClienteDestino { get; set; }

        [Display(Name = "Cuenta Destino")]
        public string NumeroCuentaDestino { get; set; }

        [Display(Name = "Monto")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Monto { get; set; }
    }
}
