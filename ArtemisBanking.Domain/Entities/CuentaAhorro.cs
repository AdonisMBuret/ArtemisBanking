using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ArtemisBanking.Domain.Entities
{
    public class CuentaAhorro : EntidadBase
    {
        // Número único de 9 dígitos que identifica la cuenta
        public string NumeroCuenta { get; set; }

        // Balance actual de la cuenta
        public decimal Balance { get; set; }

        // Indica si es la cuenta principal del cliente
        public bool EsPrincipal { get; set; }

        // Estado de la cuenta (activa o cancelada)
        public bool EstaActiva { get; set; } = true;

        // Relación: Esta cuenta pertenece a un usuario
        public string UsuarioId { get; set; }
        public virtual Usuario Usuario { get; set; }

        // Relación: Una cuenta puede tener muchas transacciones
        public virtual ICollection<Transaccion> Transacciones { get; set; }

        public CuentaAhorro()
        {
            Transacciones = new List<Transaccion>();
        }
    
    }
}
