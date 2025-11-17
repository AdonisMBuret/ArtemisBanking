
namespace ArtemisBanking.Domain.Entities
{
    public class TarjetaCredito : EntidadBase
    {
        // Número único de 16 dígitos de la tarjeta
        public string NumeroTarjeta { get; set; }

        // Límite de crédito aprobado para la tarjeta
        public decimal LimiteCredito { get; set; }

        // Deuda actual acumulada en la tarjeta
        public decimal DeudaActual { get; set; } = 0;

        // Fecha de expiración en formato MM/AA
        public string FechaExpiracion { get; set; }

        // Código de seguridad CVC cifrado con SHA-256
        public string CVC { get; set; }

        // Estado de la tarjeta (activa o cancelada)
        public bool EstaActiva { get; set; } = true;

        // Relación: Esta tarjeta pertenece a un cliente
        public string ClienteId { get; set; }
        public virtual Usuario Cliente { get; set; }

        // Relación: El administrador que asignó la tarjeta
        public string AdministradorId { get; set; }
        public virtual Usuario Administrador { get; set; }

        // Relación: Una tarjeta puede tener muchos consumos
        public virtual ICollection<ConsumoTarjeta> Consumos { get; set; }

        public TarjetaCredito()
        {
            Consumos = new List<ConsumoTarjeta>();
        }

        // Propiedad calculada: Crédito disponible
        public decimal CreditoDisponible => LimiteCredito - DeudaActual;

        // Obtiene los últimos 4 dígitos de la tarjeta para mostrar
        public string UltimosCuatroDigitos => NumeroTarjeta?.Length >= 4
            ? NumeroTarjeta.Substring(NumeroTarjeta.Length - 4)
            : NumeroTarjeta;
    }
}
