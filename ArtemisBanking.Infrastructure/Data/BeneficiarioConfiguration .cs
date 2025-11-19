using ArtemisBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBanking.Infrastructure.Data
{
    public class BeneficiarioConfiguration : IEntityTypeConfiguration<Beneficiario>
    {
        public void Configure(EntityTypeBuilder<Beneficiario> builder)
        {
            builder.ToTable("Beneficiarios");

            builder.Property(b => b.NumeroCuentaBeneficiario)
                .IsRequired()
                .HasMaxLength(9);

            builder.HasOne(b => b.CuentaAhorro)
                .WithMany()
                .HasForeignKey(b => b.CuentaAhorroId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(b => new { b.UsuarioId, b.NumeroCuentaBeneficiario })
                .IsUnique();
        }
    }
}