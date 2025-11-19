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

            builder.Property(c => c.NumeroCuenta)
                .IsRequired()
                .HasMaxLength(9);

            builder.Property(c => c.Balance)
                .HasColumnType("decimal(18,2)");

            builder.HasIndex(c => c.NumeroCuenta)
                .IsUnique();

            builder.HasMany(c => c.Transacciones)
                .WithOne(t => t.CuentaAhorro)
                .HasForeignKey(t => t.CuentaAhorroId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
