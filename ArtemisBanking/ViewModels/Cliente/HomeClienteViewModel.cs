using System.Collections.Generic;

namespace ArtemisBanking.Web.ViewModels.Cliente
{
    /// <summary>
    /// ViewModel para el home del cliente (listado de productos)
    /// </summary>
    public class HomeClienteViewModel
    {
        public IEnumerable<CuentaClienteViewModel> CuentasAhorro { get; set; }
        public IEnumerable<PrestamoClienteViewModel> Prestamos { get; set; }
        public IEnumerable<TarjetaClienteViewModel> TarjetasCredito { get; set; }

        public bool TienePrestamos => Prestamos != null && Prestamos.Any();
        public bool TieneTarjetas => TarjetasCredito != null && TarjetasCredito.Any();
    }

    public class CuentaClienteViewModel
    {
        public int Id { get; set; }
        public string NumeroCuenta { get; set; }
        public decimal Balance { get; set; }
        public bool EsPrincipal { get; set; }
        public string TipoCuenta => EsPrincipal ? "Principal" : "Secundaria";
    }

    public class PrestamoClienteViewModel
    {
        public int Id { get; set; }
        public string NumeroPrestamo { get; set; }
        public decimal MontoCapital { get; set; }
        public int TotalCuotas { get; set; }
        public int CuotasPagadas { get; set; }
        public decimal MontoPendiente { get; set; }
        public decimal TasaInteresAnual { get; set; }
        public int PlazoMeses { get; set; }
        public bool EstaAlDia { get; set; }
    }

    public class TarjetaClienteViewModel
    {
        public int Id { get; set; }
        public string NumeroTarjeta { get; set; }
        public string UltimosCuatroDigitos { get; set; }
        public decimal LimiteCredito { get; set; }
        public string FechaExpiracion { get; set; }
        public decimal DeudaActual { get; set; }
    }
}