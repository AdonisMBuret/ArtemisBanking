using ArtemisBanking.Application.Interfaces.Repositories;
using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            // Contar total de registros
            var total = await query.CountAsync();

            // Obtener usuarios de la página actual ordenados por fecha de creación (más reciente primero)
            var usuarios = await query
                .OrderByDescending(u => u.FechaCreacion)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();

            return (usuarios, total);
        }

        /// <summary>
        /// Verifica si un usuario puede ser editado
        /// Un administrador no puede editar su propia cuenta
        /// </summary>
        public async Task<bool> PuedeEditarUsuarioAsync(string usuarioId, string usuarioActualId)
        {
            return usuarioId != usuarioActualId;
        }
    }
}
