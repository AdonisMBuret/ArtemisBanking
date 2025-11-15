using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ArtemisBanking.Application.ViewModels.Cliente
{
    public class AvanceEfectivoViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar una tarjeta")]
        [Display(Name = "Tarjeta de Crédito Origen")]
        public int TarjetaId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una cuenta destino")]
        [Display(Name = "Cuenta de Ahorro Destino")]
        public int CuentaDestinoId { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto del Avance")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Monto { get; set; }

        // Para los selectores
        public IEnumerable<SelectListItem> TarjetasDisponibles { get; set; }
        public IEnumerable<SelectListItem> CuentasDisponibles { get; set; }
    }
}
