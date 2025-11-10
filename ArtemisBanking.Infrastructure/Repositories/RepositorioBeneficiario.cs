using ArtemisBanking.Domain.Interfaces;
using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Infrastructure.Repositories
{
    public class RepositorioBeneficiario : RepositorioGenerico<Beneficiario>, IRepositorioBeneficiario
    {
        public RepositorioBeneficiario(ArtemisBankingDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Obtiene todos los beneficiarios de un usuario con la información de la cuenta
        /// </summary>
        public async Task<IEnumerable<Beneficiario>> ObtenerBeneficiariosDeUsuarioAsync(string usuarioId)
        {
            return await _context.Beneficiarios
                .Include(b => b.CuentaAhorro)
                    .ThenInclude(c => c.Usuario)
                .Where(b => b.UsuarioId == usuarioId)
                .OrderByDescending(b => b.FechaCreacion)
                .ToListAsync();
        }

        /// <summary>
        /// Verifica si un usuario ya tiene registrado un beneficiario con ese número de cuenta
        /// </summary>
        public async Task<bool> ExisteBeneficiarioAsync(string usuarioId, string numeroCuenta)
        {
            return await _context.Beneficiarios
                .AnyAsync(b => b.UsuarioId == usuarioId &&
                              b.NumeroCuentaBeneficiario == numeroCuenta);
        }
    }
}