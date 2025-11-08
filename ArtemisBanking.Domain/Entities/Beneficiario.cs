using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Domain.Entities
{
    public class Beneficiario : EntidadBase
    {
        // Número de cuenta del beneficiario (de otra cuenta de ahorro)
        public string NumeroCuentaBeneficiario { get; set; }

        // Relación: Este beneficiario pertenece a un usuario
        public string UsuarioId { get; set; }
        public virtual Usuario Usuario { get; set; }

        // Relación: Referencia a la cuenta de ahorro del beneficiario
        public int CuentaAhorroId { get; set; }
        public virtual CuentaAhorro CuentaAhorro { get; set; }
    }
}
