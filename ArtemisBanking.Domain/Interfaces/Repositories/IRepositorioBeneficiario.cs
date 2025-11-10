using ArtemisBanking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Domain.Interfaces.Repositories
{
    public interface IRepositorioBeneficiario : IRepositorioGenerico<Beneficiario>
    {
        Task<IEnumerable<Beneficiario>> ObtenerBeneficiariosDeUsuarioAsync(string usuarioId);
        Task<bool> ExisteBeneficiarioAsync(string usuarioId, string numeroCuenta);
    }
}