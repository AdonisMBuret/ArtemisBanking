using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Prestamo
{

    public class SeleccionarClientePrestamoViewModel
    {
        public IEnumerable<ClienteParaPrestamoViewModel> Clientes { get; set; }

        [Display(Name = "Deuda Promedio del Sistema")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal DeudaPromedio { get; set; }
    }
}
