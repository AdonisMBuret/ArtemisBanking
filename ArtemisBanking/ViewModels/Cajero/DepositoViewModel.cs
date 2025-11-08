using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Web.ViewModels.Cajero
{
    /// <summary>
    /// ViewModel para realizar un depósito
    /// </summary>
    public class DepositoViewModel
    {
        [Required(ErrorMessage = "El número de cuenta es requerido")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número de cuenta debe tener 9 dígitos")]
        [Display(Name = "Número de Cuenta Destino")]
        public string NumeroCuentaDestino { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto a Depositar")]
        public decimal Monto { get; set; }
    }

    /// <summary>
    /// ViewModel para confirmar el depósito
    /// </summary>
    public class ConfirmarDepositoViewModel
    {
        public string NombreTitular { get; set; }
        public string ApellidoTitular { get; set; }
        public string NumeroCuenta { get; set; }
        public decimal Monto { get; set; }
    }
}