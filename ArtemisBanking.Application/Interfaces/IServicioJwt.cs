using ArtemisBanking.Domain.Entities;
using System.Security.Claims;

namespace ArtemisBanking.Application.Interfaces
{
    /// <summary>
    /// Servicio para generar y validar tokens JWT
    /// </summary>
    public interface IServicioJwt
    {
        /// <summary>
        /// Genera un token JWT para un usuario autenticado
        /// </summary>
        Task<string> GenerarTokenAsync(Usuario usuario);

        /// <summary>
        /// Valida un token JWT y devuelve los claims si es válido
        /// </summary>
        ClaimsPrincipal? ValidarToken(string token);
    }
}
