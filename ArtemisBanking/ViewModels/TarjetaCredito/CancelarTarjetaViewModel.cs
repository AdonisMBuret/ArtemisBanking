namespace ArtemisBanking.Web.ViewModels.TarjetaCredito
{
    /// <summary>
    /// ViewModel para confirmar cancelación de tarjeta
    /// </summary>
    public class CancelarTarjetaViewModel
    {
        public int Id { get; set; }
        public string UltimosCuatroDigitos { get; set; }
        public string NombreCliente { get; set; }
        public decimal DeudaActual { get; set; }
        public bool TieneDeuda => DeudaActual > 0;
    }
}