using System.Collections.Generic;

namespace ArtemisBanking.Web.ViewModels.Prestamo
{
    /// <summary>
    /// ViewModel para la lista paginada de préstamos
    /// </summary>
    public class ListaPrestamosViewModel
    {
        public IEnumerable<PrestamoListaItemViewModel> Prestamos { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
        public string FiltroCedula { get; set; }
        public bool? FiltroEstado { get; set; }

        public bool TienePaginaAnterior => PaginaActual > 1;
        public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;
    }

    public class PrestamoListaItemViewModel
    {
        public int Id { get; set; }
        public string NumeroPrestamo { get; set; }
        public string NombreCliente { get; set; }
        public string ApellidoCliente { get; set; }
        public decimal MontoCapital { get; set; }
        public int TotalCuotas { get; set; }
        public int CuotasPagadas { get; set; }
        public decimal MontoPendiente { get; set; }
        public decimal TasaInteresAnual { get; set; }
        public int PlazoMeses { get; set; }
        public bool EstaAlDia { get; set; }
        public bool EstaActivo { get; set; }
    }
}