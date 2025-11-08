using System.Collections.Generic;

namespace ArtemisBanking.Web.ViewModels.TarjetaCredito
{
    /// <summary>
    /// ViewModel para la lista paginada de tarjetas de crédito
    /// </summary>
    public class ListaTarjetasViewModel
    {
        public IEnumerable<TarjetaListaItemViewModel> Tarjetas { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
        public string FiltroCedula { get; set; }
        public bool? FiltroEstado { get; set; }

        public bool TienePaginaAnterior => PaginaActual > 1;
        public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;
    }

    public class TarjetaListaItemViewModel
    {
        public int Id { get; set; }
        public string NumeroTarjeta { get; set; }
        public string UltimosCuatroDigitos { get; set; }
        public string NombreCliente { get; set; }
        public string ApellidoCliente { get; set; }
        public decimal LimiteCredito { get; set; }
        public string FechaExpiracion { get; set; }
        public decimal DeudaActual { get; set; }
        public bool EstaActiva { get; set; }
    }
}
