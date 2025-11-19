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

        public async Task<IEnumerable<Beneficiario>> ObtenerBeneficiariosDeUsuarioAsync(string usuarioId)
        {
            return await _context.Beneficiarios
                .Include(b => b.CuentaAhorro)
                    .ThenInclude(c => c.Usuario)
                .Where(b => b.UsuarioId == usuarioId)
                .OrderByDescending(b => b.FechaCreacion)
                .ToListAsync();
        }

        public async Task<bool> ExisteBeneficiarioAsync(string usuarioId, string numeroCuenta)
        {
            return await _context.Beneficiarios
                .AnyAsync(b => b.UsuarioId == usuarioId &&
                              b.NumeroCuentaBeneficiario == numeroCuenta);
        }
    }
}
//Ya hice los repositorios, push reposiotrios