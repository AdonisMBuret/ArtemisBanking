using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Web.ViewModels.Prestamo
{
    /// <summary>
    /// ViewModel para asignar un nuevo préstamo
    /// Paso 1: Seleccionar cliente
    /// </summary>
    public class SeleccionarClientePrestamoViewModel
    {
        public IEnumerable<ClienteParaPrestamoViewModel> Clientes { get; set; }
        public decimal DeudaPromedio { get; set; }
        public string FiltroCedula { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un cliente")]
        public string ClienteSeleccionadoId { get; set; }
    }

    public class ClienteParaPrestamoViewModel
    {
        public string Id { get; set; }
        public string Cedula { get; set; }
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public decimal DeudaTotal { get; set; }
    }

    /// <summary>
    /// ViewModel para configurar el préstamo
    /// Paso 2: Configurar términos del préstamo
    /// </summary>
    public class ConfigurarPrestamoViewModel
    {
        [Required]
        public string ClienteId { get; set; }

        public string NombreCliente { get; set; }
        public decimal DeudaActualCliente { get; set; }
        public decimal DeudaPromedio { get; set; }

        [Required(ErrorMessage = "El plazo es requerido")]
        [Display(Name = "Plazo (meses)")]
        public int PlazoMeses { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(100, double.MaxValue, ErrorMessage = "El monto debe ser mayor a RD$100")]
        [Display(Name = "Monto a Prestar")]
        public decimal MontoCapital { get; set; }

        [Required(ErrorMessage = "La tasa de interés es requerida")]
        [Range(0.01, 100, ErrorMessage = "La tasa debe estar entre 0.01% y 100%")]
        [Display(Name = "Tasa de Interés Anual (%)")]
        public decimal TasaInteresAnual { get; set; }

        // Plazos permitidos para el selector
        public List<int> PlazosPermitidos { get; set; } = new List<int> 
            { 6, 12, 18, 24, 30, 36, 42, 48, 54, 60 };
    }

    /// <summary>
    /// ViewModel para advertencia de cliente de alto riesgo
    /// </summary>
    public class AdvertenciaRiesgoViewModel
    {
        public string ClienteId { get; set; }
        public string NombreCliente { get; set; }
        public decimal MontoCapital { get; set; }
        public int PlazoMeses { get; set; }
        public decimal TasaInteresAnual { get; set; }
        public decimal DeudaActual { get; set; }
        public decimal DeudaPromedio { get; set; }
        public decimal DeudaDespuesDelPrestamo { get; set; }
        public string MensajeAdvertencia { get; set; }
    }
}