
namespace ArtemisBanking.Application.ViewModels.TarjetaCredito
{
    // ==================== LISTADO DE TARJETAS ====================

    /// ViewModel para el listado de tarjetas de crédito
    public class ListaTarjetasViewModel
    {
        public IEnumerable<TarjetaListaItemViewModel> Tarjetas { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
        public string FiltroCedula { get; set; }
        public bool? FiltroEstado { get; set; }
    }
}
