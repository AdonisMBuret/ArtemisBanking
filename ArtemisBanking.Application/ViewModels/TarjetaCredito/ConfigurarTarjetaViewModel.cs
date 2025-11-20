using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.TarjetaCredito
{
    public class ConfigurarTarjetaViewModel
    {
        [Required]
        public string ClienteId { get; set; }

        [Display(Name = "Cliente")]
        public string? NombreCliente { get; set; }

        [Required(ErrorMessage = "El límite de crédito es obligatorio")]
        [Range(100, double.MaxValue, ErrorMessage = "El límite debe ser mayor a RD$100")]
        [Display(Name = "Límite de Crédito")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal LimiteCredito { get; set; }
    }
}