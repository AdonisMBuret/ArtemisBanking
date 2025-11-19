
namespace ArtemisBanking.Domain.Entities
{
    public class Beneficiario : EntidadBase
    {
        public string NumeroCuentaBeneficiario { get; set; }

        public string UsuarioId { get; set; }
        public virtual Usuario Usuario { get; set; }

        public int CuentaAhorroId { get; set; }
        public virtual CuentaAhorro CuentaAhorro { get; set; }
    }
}
