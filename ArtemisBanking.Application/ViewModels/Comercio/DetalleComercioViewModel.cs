namespace ArtemisBanking.Application.ViewModels.Comercio
{
    public class DetalleComercioViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string RNC { get; set; }
        public bool EstaActivo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool TieneUsuario { get; set; }
        public int TotalConsumos { get; set; }
        public decimal MontoTotalConsumos { get; set; }
    }
}