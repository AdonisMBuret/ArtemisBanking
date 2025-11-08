using ArtemisBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Infrastructure.Data
{
    public class PrestamoConfiguration : IEntityTypeConfiguration<Prestamo>
    {
        public void Configure(EntityTypeBuilder<Prestamo> builder)
        {
            builder.ToTable("Prestamos");

            // Propiedades requeridas
            builder.Property(p => p.NumeroPrestamo)
                .IsRequired()
                .HasMaxLength(9);

            // Montos con precisión decimal
            builder.Property(p => p.MontoCapital)
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.TasaInteresAnual)
                .HasColumnType("decimal(5,2)");

            builder.Property(p => p.CuotaMensual)
                .HasColumnType("decimal(18,2)");

            // Índice único para número de préstamo
            builder.HasIndex(p => p.NumeroPrestamo)
                .IsUnique();

            // Relación con administrador que aprobó el préstamo
            builder.HasOne(p => p.Administrador)
                .WithMany()
                .HasForeignKey(p => p.AdministradorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación con tabla de amortización
            builder.HasMany(p => p.TablaAmortizacion)
                .WithOne(c => c.Prestamo)
                .HasForeignKey(c => c.PrestamoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
