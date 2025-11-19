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

            builder.Property(c => c.MontoCuota)
                .HasColumnType("decimal(18,2)");

            builder.HasIndex(c => c.FechaPago);

            builder.HasIndex(c => new { c.PrestamoId, c.EstaPagada, c.FechaPago });
        }
    }
}