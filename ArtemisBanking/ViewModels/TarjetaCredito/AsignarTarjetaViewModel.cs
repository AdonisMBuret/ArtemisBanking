using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Web.ViewModels.TarjetaCredito
{
    /// <summary>
    /// ViewModel para asignar tarjeta de crédito
    /// Paso 1: Seleccionar cliente
    /// </summary>
    public class SeleccionarClienteTarjetaViewModel
    {
        public IEnumerable<ClienteParaTarjetaViewModel> Clientes { get; set; }
        public decimal DeudaPromedio { get; set; }
        public string FiltroCedula { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un cliente")]
        public string ClienteSeleccionadoId { get; set; }
    }

    public class ClienteParaTarjetaViewModel
    {
        public string Id { get; set; }
        public string Cedula { get; set; }
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public decimal DeudaTotal { get; set; }
    }

    /// <summary>
    /// ViewModel para configurar la tarjeta
    /// Paso 2: Configurar límite de crédito
    /// </summary>
    public class ConfigurarTarjetaViewModel
    {
        [Required]
        public string ClienteId { get; set; }

        public string NombreCliente { get; set; }

        [Required(ErrorMessage = "El límite de crédito es requerido")]
        [Range(100, double.MaxValue, ErrorMessage = "El límite debe ser mayor a RD$100")]
        [Display(Name = "Límite de Crédito")]
        public decimal LimiteCredito { get; set; }
    }
}