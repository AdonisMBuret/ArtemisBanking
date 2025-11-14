using ArtemisBanking.ViewModels.Cliente;
using System.Collections.Generic;

namespace ArtemisBanking.Web.ViewModels.Cliente
{
   /// ==================== HOME DEL CLIENTE ====================
    
    /// <summary>
    /// ViewModel principal del home del cliente
    /// Muestra todos sus productos financieros
    /// </summary>
    public class HomeClienteViewModel
    {
        public IEnumerable<CuentaClienteViewModel> CuentasAhorro { get; set; }
        public IEnumerable<PrestamoClienteViewModel> Prestamos { get; set; }
        public IEnumerable<TarjetaClienteViewModel> TarjetasCredito { get; set; }
    }
}