using ArtemisBanking.Domain.Entities;
using System.Linq.Expressions;

namespace ArtemisBanking.Domain.Interfaces.Repositories
{
    
    public interface IRepositorioUsuario
    {
        // BÚSQUEDAS BÁSICAS 

        Task<Usuario> ObtenerPorNombreUsuarioAsync(string nombreUsuario);

        Task<Usuario> ObtenerPorCorreoAsync(string correo);

        Task<Usuario> ObtenerPorCedulaAsync(string cedula);

        Task<Usuario> ObtenerPorIdAsync(string usuarioId);

        // PAGINACIÓN 

        Task<(IEnumerable<Usuario> usuarios, int total)> ObtenerUsuariosPaginadosAsync(
            int pagina,
            int tamano,
            string rol = null);

        // CONTADORES 

        Task<int> ContarAsync(Expression<Func<Usuario, bool>> predicate);

        Task<int> ContarTodosAsync();

        Task<bool> ExisteAsync(Expression<Func<Usuario, bool>> predicate);

        // VALIDACIÓN DE PERMISOS 

        Task<bool> PuedeEditarUsuarioAsync(string usuarioId, string usuarioActualId);
    }
}