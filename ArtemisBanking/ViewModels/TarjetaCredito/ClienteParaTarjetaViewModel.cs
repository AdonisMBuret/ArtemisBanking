using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.ViewModels.TarjetaCredito
{
    // ==================== CLIENTE PARA TARJETA ====================
    /// <summary>
    /// ViewModel que representa la información básica de un cliente para asignarle una tarjeta de crédito
    /// </summary>
}
public class ClienteParaTarjetaViewModel
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