using ArtemisBanking.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Application.Interfaces
{
    public interface IServicioBeneficiario
    {
        /// <summary>
        /// Agrega un nuevo beneficiario para el usuario
        /// Valida que la cuenta existe y no esté ya registrada
        /// </summary>
        Task<ResultadoOperacion> AgregarBeneficiarioAsync(string usuarioId, string numeroCuenta);

        /// <summary>
        /// Elimina un beneficiario del usuario
        /// Valida que el beneficiario pertenezca al usuario
        /// </summary>
        Task<ResultadoOperacion> EliminarBeneficiarioAsync(int beneficiarioId, string usuarioId);

        /// <summary>
        /// Obtiene todos los beneficiarios de un usuario
        /// </summary>
        Task<ResultadoOperacion<IEnumerable<BeneficiarioDTO>>> ObtenerBeneficiariosAsync(string usuarioId);
    }
}
