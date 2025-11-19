using ArtemisBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBanking.Infrastructure.Data
{
    
    public class TransaccionConfiguration : IEntityTypeConfiguration<Transaccion>
    {
        public void Configure(EntityTypeBuilder<Transaccion> builder)
        {
            builder.ToTable("Transacciones");

            builder.Property(t => t.TipoTransaccion)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(t => t.EstadoTransaccion)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(t => t.Beneficiario)
                .HasMaxLength(200);

            builder.Property(t => t.Origen)
                .HasMaxLength(200);

            builder.Property(t => t.Monto)
                .HasColumnType("decimal(18,2)");

            builder.HasIndex(t => t.FechaTransaccion);
            builder.HasIndex(t => new { t.CuentaAhorroId, t.FechaTransaccion });
        }
    }
}