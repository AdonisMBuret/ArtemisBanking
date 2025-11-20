using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Cliente
{
    public class ConsumoTarjetaClienteViewModel
    {
        [Display(Name = "Fecha")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime FechaConsumo { get; set; }

        [Display(Name = "Monto")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Monto { get; set; }

        [Display(Name = "Comercio")]
        public string NombreComercio { get; set; }

        [Display(Name = "Estado")]
        public string EstadoConsumo { get; set; }
    }
}
