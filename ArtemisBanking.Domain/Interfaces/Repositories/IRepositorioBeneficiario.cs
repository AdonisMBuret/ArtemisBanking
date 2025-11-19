using ArtemisBanking.Domain.Entities;

namespace ArtemisBanking.Domain.Interfaces.Repositories
{
    public interface IRepositorioBeneficiario : IRepositorioGenerico<Beneficiario>
    {
        Task<IEnumerable<Beneficiario>> ObtenerBeneficiariosDeUsuarioAsync(string usuarioId);
        Task<bool> ExisteBeneficiarioAsync(string usuarioId, string numeroCuenta);
    }
}

//Ya hice las interfaces, push Interface