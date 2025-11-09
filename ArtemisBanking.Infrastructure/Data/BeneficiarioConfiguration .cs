using ArtemisBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBanking.Infrastructure.Data
{
    /// <summary>
    /// Configuración de la entidad Beneficiario 
    /// </summary>
    public class BeneficiarioConfiguration : IEntityTypeConfiguration<Beneficiario>
    {
        public void Configure(EntityTypeBuilder<Beneficiario> builder)
        {
            builder.ToTable("Beneficiarios");

            // Propiedades requeridas
            builder.Property(b => b.NumeroCuentaBeneficiario)
                .IsRequired()
                .HasMaxLength(9);

            // Relación con la cuenta de ahorro del beneficiario
            builder.HasOne(b => b.CuentaAhorro)
                .WithMany()
                .HasForeignKey(b => b.CuentaAhorroId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índice compuesto para evitar beneficiarios duplicados
            builder.HasIndex(b => new { b.UsuarioId, b.NumeroCuentaBeneficiario })
                .IsUnique();
        }
    }
}