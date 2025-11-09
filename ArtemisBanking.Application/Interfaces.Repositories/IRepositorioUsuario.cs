using ArtemisBanking.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtemisBanking.Application.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz específica para operaciones con usuarios
    /// Extiende el repositorio genérico con métodos personalizados
    /// </summary>
    public interface IRepositorioUsuario
    {
        // Obtener usuario por nombre de usuario
        Task<Usuario> ObtenerPorNombreUsuarioAsync(string nombreUsuario);
        
        // Obtener usuario por correo
        Task<Usuario> ObtenerPorCorreoAsync(string correo);
        
        // Obtener usuario por cédula
        Task<Usuario> ObtenerPorCedulaAsync(string cedula);
        
        // Obtener usuarios paginados por rol
        Task<(IEnumerable<Usuario> usuarios, int total)> ObtenerUsuariosPaginadosAsync(
            int pagina, 
            int tamano, 
            string rol = null);
        
        // Verificar si un usuario puede ser editado (no es el usuario actual logueado)
        Task<bool> PuedeEditarUsuarioAsync(string usuarioId, string usuarioActualId);
    }
}