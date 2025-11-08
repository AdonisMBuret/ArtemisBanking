using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArtemisBanking.Web.ViewModels.Cliente
{
    /// <summary>
    /// ViewModel para transferencia entre cuentas propias
    /// </summary>
    public class TransferenciaEntreCuentasViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar una cuenta de origen")]
        [Display(Name = "Cuenta de Origen")]
        public int CuentaOrigenId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una cuenta de destino")]
        [Display(Name = "Cuenta de Destino")]
        public int CuentaDestinoId { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto a Transferir")]
        public decimal Monto { get; set; }

        public IEnumerable<SelectListItem> CuentasDisponibles { get; set; }
    }
}