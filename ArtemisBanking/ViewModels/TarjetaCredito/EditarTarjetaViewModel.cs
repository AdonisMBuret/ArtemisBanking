using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Web.ViewModels.TarjetaCredito
{
    /// <summary>
    /// ViewModel para editar el límite de una tarjeta
    /// </summary>
    public class EditarTarjetaViewModel
    {
        [Required]
        public int Id { get; set; }

        public string NumeroTarjeta { get; set; }
        public string UltimosCuatroDigitos { get; set; }
        public string NombreCliente { get; set; }
        public decimal DeudaActual { get; set; }

        [Required(ErrorMessage = "El límite de crédito es requerido")]
        [Range(100, double.MaxValue, ErrorMessage = "El límite debe ser mayor a RD$100")]
        [Display(Name = "Límite de Crédito")]
        public decimal LimiteCredito { get; set; }
    }
}
