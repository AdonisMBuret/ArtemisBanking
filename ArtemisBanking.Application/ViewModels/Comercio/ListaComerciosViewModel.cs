namespace ArtemisBanking.Application.ViewModels.Comercio
{
    public class ListaComerciosViewModel
    {
        public IEnumerable<ComercioItemViewModel> Comercios { get; set; } = new List<ComercioItemViewModel>();
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
    }

    public class ComercioItemViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string RNC { get; set; }
        public bool EstaActivo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool TieneUsuario { get; set; }
    }
}