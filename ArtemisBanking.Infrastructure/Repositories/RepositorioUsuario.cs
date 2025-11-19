using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Domain.Interfaces.Repositories;
using ArtemisBanking.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ArtemisBanking.Infrastructure.Repositories
{
         
    public class RepositorioUsuario : IRepositorioUsuario
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly ArtemisBankingDbContext _context;

        public RepositorioUsuario(UserManager<Usuario> userManager, ArtemisBankingDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // BÚSQUEDAS BÁSICAS 

         
        public async Task<Usuario> ObtenerPorNombreUsuarioAsync(string nombreUsuario)
        {
            return await _userManager.FindByNameAsync(nombreUsuario);
        }
         
        public async Task<Usuario> ObtenerPorCorreoAsync(string correo)
        {
            return await _userManager.FindByEmailAsync(correo);
        }
         
        public async Task<Usuario> ObtenerPorCedulaAsync(string cedula)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Cedula == cedula);
        }

        public async Task<Usuario> ObtenerPorIdAsync(string usuarioId)
        {
            return await _userManager.FindByIdAsync(usuarioId);
        }

        // PAGINACIÓN 

         
        public async Task<(IEnumerable<Usuario> usuarios, int total)> ObtenerUsuariosPaginadosAsync(
            int pagina,
            int tamano,
            string rol = null)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(rol))
            {
                var usuariosEnRol = await _userManager.GetUsersInRoleAsync(rol);
                var idsUsuariosEnRol = usuariosEnRol.Select(u => u.Id).ToList();
                query = query.Where(u => idsUsuariosEnRol.Contains(u.Id));
            }

            var total = await query.CountAsync();

            var usuarios = await query
                .OrderByDescending(u => u.FechaCreacion)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();

            return (usuarios, total);
        }

        // CONTADORES 
         
        public async Task<int> ContarAsync(Expression<Func<Usuario, bool>> predicate)
        {
            return await _context.Users
                .Where(predicate)
                .CountAsync();
        }


        public async Task<int> ContarTodosAsync()
        {
            return await _context.Users.CountAsync();
        }

        public async Task<bool> ExisteAsync(Expression<Func<Usuario, bool>> predicate)
        {
            return await _context.Users.AnyAsync(predicate);
        }

        // VALIDACIÓN DE PERMISOS 


        public async Task<bool> PuedeEditarUsuarioAsync(string usuarioId, string usuarioActualId)
        {
            // No es necesario async aquí, pero lo mantenemos por consistencia
            return await Task.FromResult(usuarioId != usuarioActualId);
        }
    }
}