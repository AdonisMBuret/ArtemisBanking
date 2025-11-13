using ArtemisBanking.Domain.Entities;

namespace ArtemisBanking.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Repositorio especializado para operaciones sobre Usuario
    /// Solo contiene métodos de acceso a datos, NO lógica de negocio
    /// </summary>
    public interface IRepositorioUsuario
    {
        // ==================== BÚSQUEDAS BÁSICAS ====================
        /// <summary>
        /// Busca un usuario por su nombre de usuario
        /// </summary>
        Task<Usuario> ObtenerPorNombreUsuarioAsync(string nombreUsuario);

        /// <summary>
        /// Busca un usuario por su correo electrónico
        /// </summary>
        Task<Usuario> ObtenerPorCorreoAsync(string correo);

        /// <summary>
        /// Busca un usuario por su cédula
        /// </summary>
        Task<Usuario> ObtenerPorCedulaAsync(string cedula);

        // ==================== PAGINACIÓN ====================
        /// <summary>
        /// Obtiene usuarios paginados con filtro opcional por rol
        /// Retorna los usuarios y el total de registros
        /// </summary>
        Task<(IEnumerable<Usuario> usuarios, int total)> ObtenerUsuariosPaginadosAsync(
            int pagina,
            int tamano,
            string rol = null);

        // ==================== VALIDACIÓN DE PERMISOS ====================
        /// <summary>
        /// Verifica si un usuario puede editar a otro usuario
        /// Un administrador no puede editar su propia cuenta
        /// </summary>
        Task<bool> PuedeEditarUsuarioAsync(string usuarioId, string usuarioActualId);
    }
}