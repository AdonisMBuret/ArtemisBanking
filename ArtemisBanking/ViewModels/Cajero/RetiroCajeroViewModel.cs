using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.ViewModels.Cajero
{
    // ==================== RETIRO ====================

    /// <summary>
    /// ViewModel para realizar un retiro de una cuenta
    /// </summary>
    public class RetiroCajeroViewModel
    {
        [Required(ErrorMessage = "El número de cuenta origen es obligatorio")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número de cuenta debe tener 9 dígitos")]
        [Display(Name = "Número de Cuenta Origen")]
        public string NumeroCuentaOrigen { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto a Retirar")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Monto { get; set; }
    }
}
