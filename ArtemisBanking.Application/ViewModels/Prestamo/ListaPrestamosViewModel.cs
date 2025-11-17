
namespace ArtemisBanking.Application.ViewModels.Prestamo
{
    // ==================== LISTADO DE PRÉSTAMOS ====================

    /// ViewModel para el listado paginado de préstamos
    public class ListaPrestamosViewModel
    {
        public IEnumerable<PrestamoListaItemViewModel> Prestamos { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
        public string FiltroCedula { get; set; }
        public bool? FiltroEstado { get; set; }
    }
}
