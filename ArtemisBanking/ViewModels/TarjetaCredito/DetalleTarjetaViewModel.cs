using System.Collections.Generic;

namespace ArtemisBanking.Web.ViewModels.TarjetaCredito
{
    /// <summary>
    /// ViewModel para ver el detalle de la tarjeta (consumos)
    /// </summary>
    public class DetalleTarjetaViewModel
    {
        public int Id { get; set; }
        public string NumeroTarjeta { get; set; }
        public string UltimosCuatroDigitos { get; set; }
        public string NombreCliente { get; set; }
        public decimal LimiteCredito { get; set; }
        public decimal DeudaActual { get; set; }
        public decimal CreditoDisponible { get; set; }
        public string FechaExpiracion { get; set; }
        public bool EstaActiva { get; set; }

        public IEnumerable<ConsumoTarjetaViewModel> Consumos { get; set; }
    }

    public class ConsumoTarjetaViewModel
    {
        public int Id { get; set; }
        public string FechaConsumo { get; set; }
        public decimal Monto { get; set; }
        public string NombreComercio { get; set; }
        public string EstadoConsumo { get; set; }
    }
}