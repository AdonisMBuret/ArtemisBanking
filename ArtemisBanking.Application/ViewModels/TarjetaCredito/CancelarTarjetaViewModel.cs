using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.TarjetaCredito
{
    // ==================== CANCELAR TARJETA ====================

    public class CancelarTarjetaViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Últimos 4 Dígitos")]
        public string? UltimosCuatroDigitos { get; set; }

        [Display(Name = "Cliente")]
        public string? NombreCliente { get; set; }

        [Display(Name = "Deuda Actual")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal DeudaActual { get; set; }
    }
}
