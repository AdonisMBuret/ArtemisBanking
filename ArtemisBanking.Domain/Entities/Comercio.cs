namespace ArtemisBanking.Domain.Entities
{
    /// <summary>
    /// Representa un comercio que puede recibir pagos a través del sistema
    /// </summary>
    public class Comercio : EntidadBase
    {
        /// <summary>
        /// Nombre del comercio
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// RNC (Registro Nacional de Contribuyentes) del comercio
        /// </summary>
        public string RNC { get; set; } = string.Empty;

        /// <summary>
        /// Indica si el comercio está activo
        /// </summary>
        public bool EstaActivo { get; set; } = true;

        /// <summary>
        /// Fecha de creación del comercio
        /// </summary>
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        /// <summary>
        /// Usuario asociado al comercio (con rol comercio)
        /// </summary>
        public string? UsuarioId { get; set; }
        public virtual Usuario? Usuario { get; set; }

        /// <summary>
        /// Consumos realizados en este comercio
        /// </summary>
        public virtual ICollection<ConsumoTarjeta> Consumos { get; set; }

        public Comercio()
        {
            Consumos = new List<ConsumoTarjeta>();
        }
    }
}
