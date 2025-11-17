using ArtemisBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBanking.Infrastructure.Data
{
    public class CuotaPrestamoConfiguration : IEntityTypeConfiguration<CuotaPrestamo>
    {
        public void Configure(EntityTypeBuilder<CuotaPrestamo> builder)
        {
            builder.ToTable("CuotasPrestamo");

            // Monto con precisión decimal
            builder.Property(c => c.MontoCuota)
                .HasColumnType("decimal(18,2)");

            // Índice para buscar cuotas por fecha
            builder.HasIndex(c => c.FechaPago);

            // Índice compuesto para buscar cuotas pendientes
            builder.HasIndex(c => new { c.PrestamoId, c.EstaPagada, c.FechaPago });
        }
    }
}