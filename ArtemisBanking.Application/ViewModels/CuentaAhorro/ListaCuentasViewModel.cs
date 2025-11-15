
namespace ArtemisBanking.Application.ViewModels.CuentaAhorro
{
    public class ListaCuentasViewModel
    {
        public IEnumerable<CuentaListaItemViewModel> Cuentas { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
        public string FiltroCedula { get; set; }
        public bool? FiltroEstado { get; set; }
        public bool? FiltroTipo { get; set; } // true = principal, false = secundaria
    }
}
