using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Web.ViewModels.Cajero
{
    /// <summary>
    /// ViewModel para transacción entre cuentas de terceros
    /// </summary>
    public class TransaccionTercerosCajeroViewModel
    {
        [Required(ErrorMessage = "El número de cuenta origen es requerido")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número de cuenta debe tener 9 dígitos")]
        [Display(Name = "Número de Cuenta Origen")]
        public string NumeroCuentaOrigen { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto de la Transacción")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "El número de cuenta destino es requerido")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número de cuenta debe tener 9 dígitos")]
        [Display(Name = "Número de Cuenta Destino")]
        public string NumeroCuentaDestino { get; set; }
    }

    /// <summary>
    /// ViewModel para confirmar transacción entre terceros
    /// </summary>
    public class ConfirmarTransaccionTercerosViewModel
    {
        public string NombreOrigen { get; set; }
        public string ApellidoOrigen { get; set; }
        public string NumeroCuentaOrigen { get; set; }
        public string NombreDestino { get; set; }
        public string ApellidoDestino { get; set; }
        public string NumeroCuentaDestino { get; set; }
        public decimal Monto { get; set; }
    }
}