using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;   

namespace ArtemisBanking.Web.ViewModels.Cliente
{
    /// <summary>
    /// ViewModel para pago a tarjeta de crédito
    /// </summary>
    public class PagoTarjetaViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar una tarjeta")]
        [Display(Name = "Tarjeta de Crédito")]
        public int TarjetaId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una cuenta de origen")]
        [Display(Name = "Cuenta de Origen")]
        public int CuentaOrigenId { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto a Pagar")]
        public decimal Monto { get; set; }

        public IEnumerable<SelectListItem> TarjetasDisponibles { get; set; }
        public IEnumerable<SelectListItem> CuentasDisponibles { get; set; }
    }
}