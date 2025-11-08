using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Web.ViewModels.Cajero
{
    /// <summary>
    /// ViewModel para realizar un retiro
    /// </summary>
    public class RetiroViewModel
    {
        [Required(ErrorMessage = "El número de cuenta es requerido")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número de cuenta debe tener 9 dígitos")]
        [Display(Name = "Número de Cuenta Origen")]
        public string NumeroCuentaOrigen { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto a Retirar")]
        public decimal Monto { get; set; }
    }

    /// <summary>
    /// ViewModel para confirmar el retiro
    /// </summary>
    public class ConfirmarRetiroViewModel
    {
        public string NombreTitular { get; set; }
        public string ApellidoTitular { get; set; }
        public string NumeroCuenta { get; set; }
        public decimal Monto { get; set; }
        public decimal BalanceActual { get; set; }
    }
}