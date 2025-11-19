using ArtemisBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBanking.Infrastructure.Data
{
    public class ConsumoTarjetaConfiguration : IEntityTypeConfiguration<ConsumoTarjeta>
    {
        public void Configure(EntityTypeBuilder<ConsumoTarjeta> builder)
        {
            builder.ToTable("ConsumosTarjeta");

            builder.Property(c => c.NombreComercio)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.EstadoConsumo)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(c => c.Monto)
                .HasColumnType("decimal(18,2)");

            builder.HasIndex(c => c.FechaConsumo);
        }
    }
}
