namespace ArtemisBanking.Domain.Entities
{
    public class Transaccion : EntidadBase
    {
        public DateTime FechaTransaccion { get; set; } = DateTime.Now;

        public decimal Monto { get; set; }

        public string TipoTransaccion { get; set; }

        public string Beneficiario { get; set; }

        public string Origen { get; set; }

        public string EstadoTransaccion { get; set; }

        public int CuentaAhorroId { get; set; }
        public virtual CuentaAhorro CuentaAhorro { get; set; }
    }
}
