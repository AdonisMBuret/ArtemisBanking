using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Comercio
{
    public class CrearComercioViewModel
    {
        [Required(ErrorMessage = "El nombre del comercio es obligatorio")]
        [StringLength(200, ErrorMessage = "El nombre no puede tener más de 200 caracteres")]
        [Display(Name = "Nombre del Comercio")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El RNC es obligatorio")]
        [StringLength(11, MinimumLength = 9, ErrorMessage = "El RNC debe tener entre 9 y 11 dígitos")]
        [RegularExpression(@"^\d+$", ErrorMessage = "El RNC solo puede contener números")]
        [Display(Name = "RNC")]
        public string RNC { get; set; }
    }
}