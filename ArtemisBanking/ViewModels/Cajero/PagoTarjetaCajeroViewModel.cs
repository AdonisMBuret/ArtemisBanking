using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Web.ViewModels.Cajero
{
    /// <summary>
    /// ViewModel para pago a tarjeta por cajero
    /// </summary>
    public class PagoTarjetaCajeroViewModel
    {
        [Required(ErrorMessage = "El número de cuenta origen es requerido")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número de cuenta debe tener 9 dígitos")]
        [Display(Name = "Número de Cuenta Origen")]
        public string NumeroCuentaOrigen { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto a Pagar")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "El número de tarjeta es requerido")]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "El número de tarjeta debe tener 16 dígitos")]
        [Display(Name = "Número de Tarjeta de Crédito")]
        public string NumeroTarjeta { get; set; }
    }

    /// <summary>
    /// ViewModel para confirmar pago a tarjeta
    /// </summary>
    public class ConfirmarPagoTarjetaCajeroViewModel
    {
        public string NombreTitular { get; set; }
        public string ApellidoTitular { get; set; }
        public string NumeroCuentaOrigen { get; set; }
        public string UltimosCuatroDigitosTarjeta { get; set; }
        public decimal Monto { get; set; }
        public decimal DeudaActualTarjeta { get; set; }
    }
}