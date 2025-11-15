using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Cajero
{
    // ==================== PAGO A TARJETA DE CRÉDITO ====================

    /// <summary>
    /// ViewModel para procesar un pago a tarjeta de crédito
    /// </summary>
    public class PagoTarjetaCajeroViewModel
    {
        [Required(ErrorMessage = "El número de cuenta origen es obligatorio")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número de cuenta debe tener 9 dígitos")]
        [Display(Name = "Número de Cuenta Origen")]
        public string NumeroCuentaOrigen { get; set; }

        [Required(ErrorMessage = "El número de tarjeta es obligatorio")]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "El número de tarjeta debe tener 16 dígitos")]
        [Display(Name = "Número de Tarjeta de Crédito")]
        public string NumeroTarjeta { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto a Pagar")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Monto { get; set; }
    }
}
