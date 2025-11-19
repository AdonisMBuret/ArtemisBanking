using ArtemisBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBanking.Infrastructure.Data
{
   
    public class PrestamoConfiguration : IEntityTypeConfiguration<Prestamo>
    {
        public void Configure(EntityTypeBuilder<Prestamo> builder)
        {
            builder.ToTable("Prestamos");

            builder.Property(p => p.NumeroPrestamo)
                .IsRequired()
                .HasMaxLength(9);

            builder.Property(p => p.MontoCapital)
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.TasaInteresAnual)
                .HasColumnType("decimal(5,2)");

            builder.Property(p => p.CuotaMensual)
                .HasColumnType("decimal(18,2)");

            builder.HasIndex(p => p.NumeroPrestamo)
                .IsUnique();

            builder.HasOne(p => p.Administrador)
                .WithMany()
                .HasForeignKey(p => p.AdministradorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.TablaAmortizacion)
                .WithOne(c => c.Prestamo)
                .HasForeignKey(c => c.PrestamoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}