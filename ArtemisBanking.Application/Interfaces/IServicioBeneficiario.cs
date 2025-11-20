using ArtemisBanking.Application.DTOs;

namespace ArtemisBanking.Application.Interfaces
{
    public interface IServicioBeneficiario
    {
        Task<ResultadoOperacion> AgregarBeneficiarioAsync(string usuarioId, string numeroCuenta);
         
        Task<ResultadoOperacion> EliminarBeneficiarioAsync(int beneficiarioId, string usuarioId);

        Task<ResultadoOperacion<IEnumerable<BeneficiarioDTO>>> ObtenerBeneficiariosAsync(string usuarioId);
    }
}
