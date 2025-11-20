using System.ComponentModel.DataAnnotations;


namespace ArtemisBanking.Application.ViewModels.Prestamo
{
    public class EditarPrestamoViewModel
    {
        [Required]
        public int Id { get; set; }

        [Display(Name = "Número de Préstamo")]
        public string? NumeroPrestamo { get; set; }

        [Display(Name = "Cliente")]
        public string? NombreCliente { get; set; }

        [Display(Name = "Tasa de Interés Actual")]
        [DisplayFormat(DataFormatString = "{0}%")]
        public decimal TasaInteresActual { get; set; }

        [Required(ErrorMessage = "La nueva tasa de interés es obligatoria")]
        [Range(0.01, 100, ErrorMessage = "La tasa debe estar entre 0.01% y 100%")]
        [Display(Name = "Nueva Tasa de Interés (%)")]
        public decimal NuevaTasaInteres { get; set; }
    }
}
