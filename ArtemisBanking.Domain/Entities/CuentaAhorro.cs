
namespace ArtemisBanking.Domain.Entities
{
    public class CuentaAhorro : EntidadBase
    {
        public string NumeroCuenta { get; set; }

        public decimal Balance { get; set; }

        public bool EsPrincipal { get; set; }

        public bool EstaActiva { get; set; } = true;

        public string UsuarioId { get; set; }
        public virtual Usuario Usuario { get; set; }

        public virtual ICollection<Transaccion> Transacciones { get; set; }

        public CuentaAhorro()
        {
            Transacciones = new List<Transaccion>();
        }
    
    }
}
