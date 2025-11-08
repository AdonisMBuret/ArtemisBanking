using System.Collections.Generic;

namespace ArtemisBanking.Web.ViewModels.CuentaAhorro
{
    /// <summary>
    /// ViewModel para la lista paginada de cuentas de ahorro
    /// </summary>
    public class ListaCuentasViewModel
    {
        public IEnumerable<CuentaListaItemViewModel> Cuentas { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
        public string FiltroCedula { get; set; }
        public bool? FiltroEstado { get; set; }
        public bool? FiltroTipo { get; set; }

        public bool TienePaginaAnterior => PaginaActual > 1;
        public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;
    }

    public class CuentaListaItemViewModel
    {
        public int Id { get; set; }
        public string NumeroCuenta { get; set; }
        public string NombreCliente { get; set; }
        public string ApellidoCliente { get; set; }
        public decimal Balance { get; set; }
        public bool EsPrincipal { get; set; }
        public bool EstaActiva { get; set; }
    }
}
