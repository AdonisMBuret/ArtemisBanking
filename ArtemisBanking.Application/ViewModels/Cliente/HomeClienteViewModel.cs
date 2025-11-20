
namespace ArtemisBanking.Application.ViewModels.Cliente
{
    public class HomeClienteViewModel
    {
        public IEnumerable<CuentaClienteViewModel> CuentasAhorro { get; set; }
        public IEnumerable<PrestamoClienteViewModel> Prestamos { get; set; }
        public IEnumerable<TarjetaClienteViewModel> TarjetasCredito { get; set; }
    }
}