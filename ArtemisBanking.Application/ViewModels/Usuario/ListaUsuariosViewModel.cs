

namespace ArtemisBanking.Application.ViewModels.Usuario
{
    // ==================== LISTADO DE USUARIOS ====================

    /// <summary>
    /// ViewModel para el listado paginado de usuarios
    /// </summary>
    public class ListaUsuariosViewModel
    {
        public IEnumerable<UsuarioListaItemViewModel> Usuarios { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
        public string FiltroRol { get; set; }
    }

}

