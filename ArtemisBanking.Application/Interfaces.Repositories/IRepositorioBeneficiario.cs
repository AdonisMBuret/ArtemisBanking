using ArtemisBanking.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtemisBanking.Application.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz para operaciones con beneficiarios
    /// </summary>
    public interface IRepositorioBeneficiario : IRepositorioGenerico<Beneficiario>
    {
        // Obtener todos los beneficiarios de un usuario
        Task<IEnumerable<Beneficiario>> ObtenerBeneficiariosDeUsuarioAsync(string usuarioId);
        
        // Verificar si un beneficiario ya está registrado por un usuario
        Task<bool> ExisteBeneficiarioAsync(string usuarioId, string numeroCuenta);
    }
}