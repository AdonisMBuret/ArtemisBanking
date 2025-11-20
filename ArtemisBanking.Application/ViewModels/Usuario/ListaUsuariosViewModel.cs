
namespace ArtemisBanking.Application.ViewModels.Usuario
{
    public class ListaUsuariosViewModel
    {
        public IEnumerable<UsuarioListaItemViewModel> Usuarios { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
        public string FiltroRol { get; set; }
    }
}

