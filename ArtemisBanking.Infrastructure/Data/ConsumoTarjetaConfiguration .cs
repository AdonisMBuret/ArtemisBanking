using ArtemisBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBanking.Infrastructure.Data
{
    /// Configuración de la entidad ConsumoTarjeta
    public class ConsumoTarjetaConfiguration : IEntityTypeConfiguration<ConsumoTarjeta>
    {
        public void Configure(EntityTypeBuilder<ConsumoTarjeta> builder)
        {
            builder.ToTable("ConsumosTarjeta");

            // Propiedades requeridas
            builder.Property(c => c.NombreComercio)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.EstadoConsumo)
                .IsRequired()
                .HasMaxLength(20);

            // Monto con precisión decimal
            builder.Property(c => c.Monto)
                .HasColumnType("decimal(18,2)");

            // Índice para buscar consumos por fecha
            builder.HasIndex(c => c.FechaConsumo);
        }
    }
}
