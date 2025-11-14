using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.ViewModels.Cliente
{
    /// <summary>
    /// ViewModel para cada transacción en el detalle de cuenta
    /// </summary>
    public class TransaccionClienteViewModel
    {
        [Display(Name = "Fecha")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime FechaTransaccion { get; set; }

        [Display(Name = "Monto")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Monto { get; set; }

        [Display(Name = "Tipo")]
        public string TipoTransaccion { get; set; }

        [Display(Name = "Beneficiario")]
        public string Beneficiario { get; set; }

        [Display(Name = "Origen")]
        public string Origen { get; set; }

        [Display(Name = "Estado")]
        public string EstadoTransaccion { get; set; }
    }
}
