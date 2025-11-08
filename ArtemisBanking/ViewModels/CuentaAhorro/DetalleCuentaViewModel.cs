using System.Collections.Generic;

namespace ArtemisBanking.Web.ViewModels.CuentaAhorro
{
    /// <summary>
    /// ViewModel para ver el detalle de la cuenta (transacciones)
    /// </summary>
    public class DetalleCuentaViewModel
    {
        public int Id { get; set; }
        public string NumeroCuenta { get; set; }
        public string NombreCliente { get; set; }
        public decimal Balance { get; set; }
        public bool EsPrincipal { get; set; }
        public bool EstaActiva { get; set; }

        public IEnumerable<TransaccionViewModel> Transacciones { get; set; }
    }

    public class TransaccionViewModel
    {
        public int Id { get; set; }
        public string FechaTransaccion { get; set; }
        public decimal Monto { get; set; }
        public string TipoTransaccion { get; set; }
        public string Beneficiario { get; set; }
        public string Origen { get; set; }
        public string EstadoTransaccion { get; set; }
    }
}