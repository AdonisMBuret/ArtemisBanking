using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.PortalComercio
{

    public class RegistrarConsumoViewModel
    {
        [Required(ErrorMessage = "El número de tarjeta es obligatorio")]
        [RegularExpression(@"^[0-9]{16}$", ErrorMessage = "El número de tarjeta debe tener 16 dígitos")]
        [Display(Name = "Número de Tarjeta")]
        public string NumeroTarjeta { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, 1000000, ErrorMessage = "El monto debe estar entre RD$0.01 y RD$1,000,000")]
        [Display(Name = "Monto del Consumo")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal Monto { get; set; }
    }

    public class ConfirmarConsumoViewModel
    {
        public string? NumeroTarjeta { get; set; }
        public string? UltimosCuatroDigitos { get; set; }
        public string? NombreCliente { get; set; }
        public decimal LimiteCredito { get; set; }
        public decimal DeudaActual { get; set; }
        public decimal CreditoDisponible { get; set; }
        public decimal Monto { get; set; }
    }
}
