using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Domain.Entities
{
    public class Transaccion : EntidadBase
    {
        // Fecha y hora de la transacción
        public DateTime FechaTransaccion { get; set; } = DateTime.Now;

        // Monto de la transacción
        public decimal Monto { get; set; }

        // Tipo de transacción: DÉBITO (salida) o CRÉDITO (entrada)
        public string TipoTransaccion { get; set; }

        // Destino de la transacción (número de cuenta, tarjeta, préstamo, etc.)
        public string Beneficiario { get; set; }

        // Origen de la transacción (de dónde viene el dinero)
        public string Origen { get; set; }

        // Estado de la transacción (APROBADA o RECHAZADA)
        public string EstadoTransaccion { get; set; }

        // Relación: Esta transacción pertenece a una cuenta de ahorro
        public int CuentaAhorroId { get; set; }
        public virtual CuentaAhorro CuentaAhorro { get; set; }
    }
}
