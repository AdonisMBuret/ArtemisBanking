using ArtemisBanking.Domain.Entities;
using System.Linq.Expressions;

namespace ArtemisBanking.Domain.Interfaces.Repositories
{
    
    /// Repositorio especializado para operaciones sobre Usuario
    /// Solo contiene métodos de acceso a datos, NO lógica de negocio
    public interface IRepositorioUsuario
    {
        // ==================== BÚSQUEDAS BÁSICAS ====================

        /// Busca un usuario por su nombre de usuario
        Task<Usuario> ObtenerPorNombreUsuarioAsync(string nombreUsuario);

        /// Busca un usuario por su correo electrónico
        Task<Usuario> ObtenerPorCorreoAsync(string correo);

        /// Busca un usuario por su cédula
        Task<Usuario> ObtenerPorCedulaAsync(string cedula);

        /// Busca un usuario por su ID
        Task<Usuario> ObtenerPorIdAsync(string usuarioId);

        // ==================== PAGINACIÓN ====================

        /// Obtiene usuarios paginados con filtro opcional por rol
        /// Retorna los usuarios y el total de registros
        Task<(IEnumerable<Usuario> usuarios, int total)> ObtenerUsuariosPaginadosAsync(
            int pagina,
            int tamano,
            string rol = null);

        // ==================== CONTADORES ====================

        /// Cuenta usuarios según un predicado
        /// Ejemplo: ContarAsync(u => u.EstaActivo) cuenta solo los activos
        Task<int> ContarAsync(Expression<Func<Usuario, bool>> predicate);

        /// Cuenta todos los usuarios sin filtro
        Task<int> ContarTodosAsync();

        /// Verifica si existe al menos un usuario que cumpla el predicado
        Task<bool> ExisteAsync(Expression<Func<Usuario, bool>> predicate);

        // ==================== VALIDACIÓN DE PERMISOS ====================

        /// Verifica si un usuario puede editar a otro usuario
        /// Un administrador no puede editar su propia cuenta
        Task<bool> PuedeEditarUsuarioAsync(string usuarioId, string usuarioActualId);
    }
}