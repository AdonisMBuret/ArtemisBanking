using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArtemisBanking.Web.ViewModels.Cliente
{
    /// <summary>
    /// ViewModel para avance de efectivo
    /// </summary>
    public class AvanceEfectivoViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar una tarjeta")]
        [Display(Name = "Tarjeta de Crédito Origen")]
        public int TarjetaId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una cuenta destino")]
        [Display(Name = "Cuenta de Ahorro Destino")]
        public int CuentaDestinoId { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto del Avance")]
        public decimal Monto { get; set; }

        public IEnumerable<SelectListItem> TarjetasDisponibles { get; set; }
        public IEnumerable<SelectListItem> CuentasDisponibles { get; set; }
    }
}
