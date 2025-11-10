using System.Linq.Expressions;
using ArtemisBanking.Domain.Entities;

namespace ArtemisBanking.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Repositorio especializado para operaciones sobre Usuario (extiende la funcionalidad de UserManager)
    /// </summary>
    public interface IRepositorioUsuario
    {
        // === Búsquedas básicas ===
        Task<Usuario> ObtenerPorNombreUsuarioAsync(string nombreUsuario);
        Task<Usuario> ObtenerPorCorreoAsync(string correo);
        Task<Usuario> ObtenerPorCedulaAsync(string cedula);

        // === Paginación con filtro por rol ===
        Task<(IEnumerable<Usuario> usuarios, int total)> ObtenerUsuariosPaginadosAsync(
            int pagina,
            int tamano,
            string rol = null);

        // === Seguridad ===
        Task<bool> PuedeEditarUsuarioAsync(string usuarioId, string usuarioActualId);

        // ⭐⭐⭐ MÉTODOS QUE TE FALTABAN ⭐⭐⭐

        /// <summary>
        /// Cuenta usuarios según un predicado opcional.
        /// Si no se pasa predicado, cuenta todos.
        /// </summary>
        Task<int> ContarAsync(Expression<Func<Usuario, bool>> predicate = null);

        /// <summary>
        /// Cuenta todos los usuarios (sin filtro)
        /// </summary>
        Task<int> ContarTodosAsync();

        /// <summary>
        /// Verifica si existe al menos un usuario que cumpla el predicado
        /// </summary>
        Task<bool> ExisteAsync(Expression<Func<Usuario, bool>> predicate);
    }
}