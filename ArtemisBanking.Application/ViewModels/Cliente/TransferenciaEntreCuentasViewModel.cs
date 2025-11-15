using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ArtemisBanking.Application.ViewModels.Cliente
{
    public class TransferenciaEntreCuentasViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar una cuenta de origen")]
        [Display(Name = "Cuenta de Origen")]
        public int CuentaOrigenId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una cuenta de destino")]
        [Display(Name = "Cuenta de Destino")]
        public int CuentaDestinoId { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto a Transferir")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Monto { get; set; }

        // Para los selectores
        public IEnumerable<SelectListItem> CuentasDisponibles { get; set; }
    }
}