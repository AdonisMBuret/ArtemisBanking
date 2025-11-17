
namespace ArtemisBanking.Application.ViewModels.Usuario
{
    // ==================== LISTADO DE USUARIOS ====================

    /// ViewModel para el listado paginado de usuarios
    public class ListaUsuariosViewModel
    {
        public IEnumerable<UsuarioListaItemViewModel> Usuarios { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
        public string FiltroRol { get; set; }
    }
}

