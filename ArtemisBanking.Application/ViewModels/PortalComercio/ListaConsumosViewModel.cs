using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.PortalComercio
{
    public class ListaConsumosViewModel
    {
        public IEnumerable<ConsumoItemViewModel> Consumos { get; set; } = new List<ConsumoItemViewModel>();
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
        public DateTime? FiltroFechaInicio { get; set; }
        public DateTime? FiltroFechaFin { get; set; }
    }

    public class ConsumoItemViewModel
    {
        public int Id { get; set; }
        public DateTime FechaConsumo { get; set; }
        public decimal Monto { get; set; }
        public string UltimosCuatroDigitosTarjeta { get; set; } = string.Empty;
        public string NombreCliente { get; set; } = string.Empty;
        public string EstadoConsumo { get; set; } = string.Empty;
    }
}
