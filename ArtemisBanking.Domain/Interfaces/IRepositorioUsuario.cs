using ArtemisBanking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Domain.Interfaces
{
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
