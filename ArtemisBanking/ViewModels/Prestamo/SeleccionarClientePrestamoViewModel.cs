using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.ViewModels.Prestamo
{
    // ==================== SELECCIONAR CLIENTE PARA PRÉSTAMO ====================

    /// <summary>
    /// ViewModel para mostrar el listado de clientes sin préstamo activo
    /// </summary>
    public class SeleccionarClientePrestamoViewModel
    {
        public IEnumerable<ClienteParaPrestamoViewModel> Clientes { get; set; }

        [Display(Name = "Deuda Promedio del Sistema")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal DeudaPromedio { get; set; }
    }
}
