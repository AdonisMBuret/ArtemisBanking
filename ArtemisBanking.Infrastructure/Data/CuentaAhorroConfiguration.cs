using ArtemisBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Infrastructure.Data
{
    public class CuentaAhorroConfiguration : IEntityTypeConfiguration<CuentaAhorro>
    {
        public void Configure(EntityTypeBuilder<CuentaAhorro> builder)
        {
            builder.ToTable("CuentasAhorro");

            // Propiedades requeridas
            builder.Property(c => c.NumeroCuenta)
                .IsRequired()
                .HasMaxLength(9);

            // Balance con precisión decimal (para manejar dinero correctamente)
            builder.Property(c => c.Balance)
                .HasColumnType("decimal(18,2)");

            // Índice único para número de cuenta
            builder.HasIndex(c => c.NumeroCuenta)
                .IsUnique();

            // Relación con transacciones
            builder.HasMany(c => c.Transacciones)
                .WithOne(t => t.CuentaAhorro)
                .HasForeignKey(t => t.CuentaAhorroId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
