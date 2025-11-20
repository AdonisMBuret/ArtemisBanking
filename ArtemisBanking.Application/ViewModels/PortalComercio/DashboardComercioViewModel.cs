using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.PortalComercio
{

    public class DashboardComercioViewModel
    {
        public string NombreComercio { get; set; } = string.Empty;
        public string RNC { get; set; } = string.Empty;
        public int TotalConsumosHoy { get; set; }
        public decimal MontoTotalHoy { get; set; }
        public int TotalConsumosMes { get; set; }
        public decimal MontoTotalMes { get; set; }
        public IEnumerable<ConsumoRecienteViewModel> ConsumosRecientes { get; set; } = new List<ConsumoRecienteViewModel>();
    }

    public class ConsumoRecienteViewModel
    {
        public DateTime FechaConsumo { get; set; }
        public decimal Monto { get; set; }
        public string UltimosCuatroDigitos { get; set; } = string.Empty;
        public string EstadoConsumo { get; set; } = string.Empty;
    }
}
