using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.ViewModels.TarjetaCredito
{
    public class SeleccionarClienteTarjetaViewModel
    {
        public IEnumerable<ClienteParaTarjetaViewModel> Clientes { get; set; }

        [Display(Name = "Deuda Promedio del Sistema")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal DeudaPromedio { get; set; }
    }
}
