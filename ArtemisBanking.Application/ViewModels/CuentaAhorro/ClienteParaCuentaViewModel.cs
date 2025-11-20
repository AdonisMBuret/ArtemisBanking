using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.CuentaAhorro
{
    public class ClienteParaCuentaViewModel
    {
        public string Id { get; set; }

        [Display(Name = "Cédula")]
        public string Cedula { get; set; }

        [Display(Name = "Nombre Completo")]
        public string NombreCompleto { get; set; }

        [Display(Name = "Correo")]
        public string Correo { get; set; }

        [Display(Name = "Deuda Total Actual")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal DeudaTotal { get; set; }
    }
}