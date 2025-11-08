using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Web.ViewModels.Prestamo
{
    /// <summary>
    /// ViewModel para editar la tasa de interés de un préstamo
    /// </summary>
    public class EditarPrestamoViewModel
    {
        [Required]
        public int Id { get; set; }

        public string NumeroPrestamo { get; set; }
        public string NombreCliente { get; set; }

        [Required(ErrorMessage = "La tasa de interés es requerida")]
        [Range(0.01, 100, ErrorMessage = "La tasa debe estar entre 0.01% y 100%")]
        [Display(Name = "Tasa de Interés Anual (%)")]
        public decimal TasaInteresAnual { get; set; }
    }
}