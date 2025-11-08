namespace ArtemisBanking.Web.ViewModels.CuentaAhorro
{
    /// <summary>
    /// ViewModel para confirmar cancelación de cuenta
    /// </summary>
    public class CancelarCuentaViewModel
    {
        public int Id { get; set; }
        public string NumeroCuenta { get; set; }
        public string NombreCliente { get; set; }
        public decimal Balance { get; set; }
        public bool EsPrincipal { get; set; }
        public bool TieneBalance => Balance > 0;
    }
}