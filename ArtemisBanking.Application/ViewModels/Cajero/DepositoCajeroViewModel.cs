using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Cajero
{
    // ==================== DEPÓSITO ====================

    /// <summary>
    /// ViewModel para realizar un depósito a una cuenta
    /// </summary>
    public class DepositoCajeroViewModel
    {
        [Required(ErrorMessage = "El número de cuenta destino es obligatorio")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número de cuenta debe tener 9 dígitos")]
        [Display(Name = "Número de Cuenta Destino")]
        public string NumeroCuentaDestino { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto a Depositar")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Monto { get; set; }
    }
}
