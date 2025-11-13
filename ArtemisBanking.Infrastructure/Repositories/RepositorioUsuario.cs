using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Domain.Interfaces.Repositories;
using ArtemisBanking.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ArtemisBanking.Infrastructure.Repositories
{
    /// <summary>
    /// Implementación del repositorio de usuarios
    /// Usa UserManager de Identity para operaciones básicas
    /// y DbContext para consultas más complejas
    /// </summary>
    public class RepositorioUsuario : IRepositorioUsuario
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly ArtemisBankingDbContext _context;

        public RepositorioUsuario(UserManager<Usuario> userManager, ArtemisBankingDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // ==================== BÚSQUEDAS BÁSICAS ====================

        /// <summary>
        /// Busca un usuario por su nombre de usuario
        /// </summary>
        public async Task<Usuario> ObtenerPorNombreUsuarioAsync(string nombreUsuario)
        {
            return await _userManager.FindByNameAsync(nombreUsuario);
        }

        /// <summary>
        /// Busca un usuario por su correo electrónico
        /// </summary>
        public async Task<Usuario> ObtenerPorCorreoAsync(string correo)
        {
            return await _userManager.FindByEmailAsync(correo);
        }

        /// <summary>
        /// Busca un usuario por su cédula
        /// </summary>
        public async Task<Usuario> ObtenerPorCedulaAsync(string cedula)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Cedula == cedula);
        }

        /// <summary>
        /// Busca un usuario por su ID
        /// </summary>
        public async Task<Usuario> ObtenerPorIdAsync(string usuarioId)
        {
            return await _userManager.FindByIdAsync(usuarioId);
        }

        // ==================== PAGINACIÓN ====================

        /// <summary>
        /// Obtiene usuarios paginados, opcionalmente filtrados por rol
        /// Retorna los usuarios y el total de registros para la paginación
        /// </summary>
        public async Task<(IEnumerable<Usuario> usuarios, int total)> ObtenerUsuariosPaginadosAsync(
            int pagina,
            int tamano,
            string rol = null)
        {
            var query = _context.Users.AsQueryable();

            // Si se especifica un rol, filtrar por ese rol
            if (!string.IsNullOrEmpty(rol))
            {
                var usuariosEnRol = await _userManager.GetUsersInRoleAsync(rol);
                var idsUsuariosEnRol = usuariosEnRol.Select(u => u.Id).ToList();
                query = query.Where(u => idsUsuariosEnRol.Contains(u.Id));
            }

            // Contar total de registros (antes de paginar)
            var total = await query.CountAsync();

            // Obtener usuarios de la página actual ordenados por fecha de creación (más reciente primero)
            var usuarios = await query
                .OrderByDescending(u => u.FechaCreacion)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();

            return (usuarios, total);
        }

        // ==================== CONTADORES ====================

        /// <summary>
        /// Cuenta usuarios según un predicado
        /// Ejemplo: ContarAsync(u => u.EstaActivo) cuenta solo los activos
        /// </summary>
        public async Task<int> ContarAsync(Expression<Func<Usuario, bool>> predicate)
        {
            return await _context.Users
                .Where(predicate)
                .CountAsync();
        }

        /// <summary>
        /// Cuenta todos los usuarios sin filtro
        /// </summary>
        public async Task<int> ContarTodosAsync()
        {
            return await _context.Users.CountAsync();
        }

        /// <summary>
        /// Verifica si existe al menos un usuario que cumpla el predicado
        /// </summary>
        public async Task<bool> ExisteAsync(Expression<Func<Usuario, bool>> predicate)
        {
            return await _context.Users.AnyAsync(predicate);
        }

        // ==================== VALIDACIÓN DE PERMISOS ====================

        /// <summary>
        /// Verifica si un usuario puede ser editado
        /// Un administrador no puede editar su propia cuenta
        /// </summary>
        public async Task<bool> PuedeEditarUsuarioAsync(string usuarioId, string usuarioActualId)
        {
            // No es necesario async aquí, pero lo mantenemos por consistencia
            return await Task.FromResult(usuarioId != usuarioActualId);
        }
    }
}