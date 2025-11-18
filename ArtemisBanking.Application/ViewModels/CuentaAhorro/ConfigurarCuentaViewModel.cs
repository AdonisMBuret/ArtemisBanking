using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.CuentaAhorro
{
    public class ConfigurarCuentaViewModel
    {
        [Required]
        public string ClienteId { get; set; }

        [Display(Name = "Cliente")]
        public string? NombreCliente { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El balance debe ser mayor o igual a 0")]
        [Display(Name = "Balance Inicial")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal BalanceInicial { get; set; } = 0;
    }
}
