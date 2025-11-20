using ArtemisBanking.Domain.Entities;
using System.Security.Claims;

namespace ArtemisBanking.Application.Interfaces
{

    public interface IServicioJwt
    {

        Task<string> GenerarTokenAsync(Usuario usuario);

        ClaimsPrincipal? ValidarToken(string token);
    }
}
