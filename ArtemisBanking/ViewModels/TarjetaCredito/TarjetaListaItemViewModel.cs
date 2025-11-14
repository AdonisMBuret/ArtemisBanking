using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Web.ViewModels.TarjetaCredito
{
    public class TarjetaListaItemViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Número de Tarjeta")]
        public string NumeroTarjeta { get; set; }

        [Display(Name = "Cliente")]
        public string NombreCliente { get; set; }

        [Display(Name = "Límite de Crédito")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal LimiteCredito { get; set; }

        [Display(Name = "Deuda Actual")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal DeudaActual { get; set; }

        [Display(Name = "Fecha de Expiración")]
        public string FechaExpiracion { get; set; }

        [Display(Name = "Estado")]
        public bool EstaActiva { get; set; }
    }
}