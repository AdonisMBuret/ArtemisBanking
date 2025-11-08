using System.Collections.Generic;

namespace ArtemisBanking.Web.ViewModels.Usuario
{
    /// <summary>
    /// ViewModel para la lista paginada de usuarios
    /// </summary>
    public class ListaUsuariosViewModel
    {
        public IEnumerable<UsuarioListaItemViewModel> Usuarios { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalRegistros { get; set; }
        public string FiltroRol { get; set; }

        public bool TienePaginaAnterior => PaginaActual > 1;
        public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;
    }

    /// <summary>
    /// ViewModel para cada item en la lista de usuarios
    /// </summary>
    public class UsuarioListaItemViewModel
    {
        public string Id { get; set; }
        public string NombreUsuario { get; set; }
        public string Cedula { get; set; }
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public string TipoUsuario { get; set; }
        public bool EstaActivo { get; set; }
        public bool PuedeEditar { get; set; } // No se puede editar el usuario logueado
    }
}

