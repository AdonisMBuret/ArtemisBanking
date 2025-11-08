using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArtemisBanking.Web.ViewModels.Cliente
{
    /// <summary>
    /// ViewModel para transacción express (a cualquier cuenta)
    /// </summary>
    public class TransaccionExpressViewModel
    {
        [Required(ErrorMessage = "El número de cuenta destino es requerido")]
        [StringLength(9, MinimumLength = 9, ErrorMessage = "El número de cuenta debe tener 9 dígitos")]
        [Display(Name = "Número de Cuenta Destino")]
        public string NumeroCuentaDestino { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto a Transferir")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una cuenta de origen")]
        [Display(Name = "Cuenta de Origen")]
        public int CuentaOrigenId { get; set; }

        public IEnumerable<SelectListItem> CuentasDisponibles { get; set; }
    }

    /// <summary>
    /// ViewModel para confirmar transacción
    /// </summary>
    public class ConfirmarTransaccionViewModel
    {
        public string NombreDestinatario { get; set; }
        public string ApellidoDestinatario { get; set; }
        public string NumeroCuentaDestino { get; set; }
        public decimal Monto { get; set; }
        public int CuentaOrigenId { get; set; }
        public string NumeroCuentaOrigen { get; set; }
    }
}