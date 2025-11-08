using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Web.ViewModels.CuentaAhorro
{
    /// <summary>
    /// ViewModel para asignar cuenta de ahorro secundaria
    /// Paso 1: Seleccionar cliente
    /// </summary>
    public class SeleccionarClienteCuentaViewModel
    {
        public IEnumerable<ClienteParaCuentaViewModel> Clientes { get; set; }
        public string FiltroCedula { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un cliente")]
        public string ClienteSeleccionadoId { get; set; }
    }

    public class ClienteParaCuentaViewModel
    {
        public string Id { get; set; }
        public string Cedula { get; set; }
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public decimal DeudaTotal { get; set; }
    }

    /// <summary>
    /// ViewModel para configurar la cuenta
    /// Paso 2: Configurar balance inicial
    /// </summary>
    public class ConfigurarCuentaViewModel
    {
        [Required]
        public string ClienteId { get; set; }

        public string NombreCliente { get; set; }

        [Required(ErrorMessage = "El balance inicial es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El balance debe ser mayor o igual a 0")]
        [Display(Name = "Balance Inicial")]
        public decimal BalanceInicial { get; set; }
    }
}