namespace ArtemisBanking.Domain.Entities
{

    public class Comercio : EntidadBase
    {
 
        public string Nombre { get; set; } = string.Empty;

         public string RNC { get; set; } = string.Empty;

        public bool EstaActivo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public string? UsuarioId { get; set; }
        public virtual Usuario? Usuario { get; set; }


        public virtual ICollection<ConsumoTarjeta> Consumos { get; set; }

        public Comercio()
        {
            Consumos = new List<ConsumoTarjeta>();
        }
    }
}
