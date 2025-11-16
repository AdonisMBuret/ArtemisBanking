using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArtemisBanking.Application.ViewModels.Cliente
{
    public class PagoPrestamoViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un préstamo")]
        [Display(Name = "Préstamo")]
        public int PrestamoId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una cuenta de origen")]
        [Display(Name = "Cuenta de Origen")]
        public int CuentaOrigenId { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto a Pagar")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Monto { get; set; }

        // Para los selectores
        public IEnumerable<SelectListItem> PrestamosDisponibles { get; set; }
        public IEnumerable<SelectListItem> CuentasDisponibles { get; set; }
    }
}
