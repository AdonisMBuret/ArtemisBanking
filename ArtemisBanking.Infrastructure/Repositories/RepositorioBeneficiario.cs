using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ArtemisBanking.Domain.Interfaces.Repositories;

namespace ArtemisBanking.Infrastructure.Repositories
{
    public class RepositorioBeneficiario : RepositorioGenerico<Beneficiario>, IRepositorioBeneficiario
    {
        public RepositorioBeneficiario(ArtemisBankingDbContext context) : base(context)
        {
        }

        /// Obtiene todos los beneficiarios de un usuario con la información de la cuenta
        public async Task<IEnumerable<Beneficiario>> ObtenerBeneficiariosDeUsuarioAsync(string usuarioId)
        {
            return await _context.Beneficiarios
                .Include(b => b.CuentaAhorro)
                    .ThenInclude(c => c.Usuario)
                .Where(b => b.UsuarioId == usuarioId)
                .OrderByDescending(b => b.FechaCreacion)
                .ToListAsync();
        }

        /// Verifica si un usuario ya tiene registrado un beneficiario con ese número de cuenta
        public async Task<bool> ExisteBeneficiarioAsync(string usuarioId, string numeroCuenta)
        {
            return await _context.Beneficiarios
                .AnyAsync(b => b.UsuarioId == usuarioId &&
                              b.NumeroCuentaBeneficiario == numeroCuenta);
        }
    }
}