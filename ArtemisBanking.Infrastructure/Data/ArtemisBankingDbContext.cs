using ArtemisBanking.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ArtemisBanking.Infrastructure.Data
{
    public class ArtemisBankingDbContext : IdentityDbContext<Usuario>
    {
        public ArtemisBankingDbContext(DbContextOptions<ArtemisBankingDbContext> options)
            : base(options)
        {
        }

        // DbSets para cada entidad del sistema
        public DbSet<CuentaAhorro> CuentasAhorro { get; set; }
        public DbSet<Prestamo> Prestamos { get; set; }
        public DbSet<CuotaPrestamo> CuotasPrestamo { get; set; }
        public DbSet<TarjetaCredito> TarjetasCredito { get; set; }
        public DbSet<ConsumoTarjeta> ConsumosTarjeta { get; set; }
        public DbSet<Transaccion> Transacciones { get; set; }
        public DbSet<Beneficiario> Beneficiarios { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Aplicar todas las configuraciones de entidades
            builder.ApplyConfiguration(new UsuarioConfiguration());
            builder.ApplyConfiguration(new CuentaAhorroConfiguration());
            builder.ApplyConfiguration(new PrestamoConfiguration());
            builder.ApplyConfiguration(new CuotaPrestamoConfiguration());
            builder.ApplyConfiguration(new TarjetaCreditoConfiguration());
            builder.ApplyConfiguration(new ConsumoTarjetaConfiguration());
            builder.ApplyConfiguration(new TransaccionConfiguration());
            builder.ApplyConfiguration(new BeneficiarioConfiguration());

        }
    }
}
