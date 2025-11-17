
namespace ArtemisBanking.Application.ViewModels.Cliente
{
    /// ==================== HOME DEL CLIENTE ====================

    /// ViewModel principal del home del cliente
    /// Muestra todos sus productos financieros
    public class HomeClienteViewModel
    {
        public IEnumerable<CuentaClienteViewModel> CuentasAhorro { get; set; }
        public IEnumerable<PrestamoClienteViewModel> Prestamos { get; set; }
        public IEnumerable<TarjetaClienteViewModel> TarjetasCredito { get; set; }
    }
}