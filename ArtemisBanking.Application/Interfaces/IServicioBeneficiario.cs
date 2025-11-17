using ArtemisBanking.Application.DTOs;

namespace ArtemisBanking.Application.Interfaces
{
    public interface IServicioBeneficiario
    {
         
        /// Agrega un nuevo beneficiario para el usuario
        /// Valida que la cuenta existe y no esté ya registrada
         
        Task<ResultadoOperacion> AgregarBeneficiarioAsync(string usuarioId, string numeroCuenta);

         
        /// Elimina un beneficiario del usuario
        /// Valida que el beneficiario pertenezca al usuario
         
        Task<ResultadoOperacion> EliminarBeneficiarioAsync(int beneficiarioId, string usuarioId);

         
        /// Obtiene todos los beneficiarios de un usuario
         
        Task<ResultadoOperacion<IEnumerable<BeneficiarioDTO>>> ObtenerBeneficiariosAsync(string usuarioId);
    }
}
