using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.TarjetaCredito
{
    // ==================== EDITAR TARJETA ====================

    public class EditarTarjetaViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Número de Tarjeta")]
        public string NumeroTarjeta { get; set; }

        [Display(Name = "Últimos 4 Dígitos")]
        public string UltimosCuatroDigitos { get; set; }

        [Display(Name = "Cliente")]
        public string NombreCliente { get; set; }

        [Display(Name = "Deuda Actual")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal DeudaActual { get; set; }

        [Required(ErrorMessage = "El límite de crédito es obligatorio")]
        [Range(100, double.MaxValue, ErrorMessage = "El límite debe ser mayor a RD$100")]
        [Display(Name = "Límite de Crédito")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal LimiteCredito { get; set; }
    }
}
