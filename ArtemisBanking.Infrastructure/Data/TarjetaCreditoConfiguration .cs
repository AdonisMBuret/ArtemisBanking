using ArtemisBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBanking.Infrastructure.Data
{
    /// Configuración de la entidad TarjetaCredito
    public class TarjetaCreditoConfiguration : IEntityTypeConfiguration<TarjetaCredito>
    {
        public void Configure(EntityTypeBuilder<TarjetaCredito> builder)
        {
            builder.ToTable("TarjetasCredito");

            // Propiedades requeridas
            builder.Property(t => t.NumeroTarjeta)
                .IsRequired()
                .HasMaxLength(16);

            builder.Property(t => t.FechaExpiracion)
                .IsRequired()
                .HasMaxLength(5); // MM/AA

            builder.Property(t => t.CVC)
                .IsRequired()
                .HasMaxLength(256); // CVC cifrado con SHA-256

            // Montos con precisión decimal
            builder.Property(t => t.LimiteCredito)
                .HasColumnType("decimal(18,2)");

            builder.Property(t => t.DeudaActual)
                .HasColumnType("decimal(18,2)");

            // Índice único para número de tarjeta
            builder.HasIndex(t => t.NumeroTarjeta)
                .IsUnique();

            // Relación con administrador que asignó la tarjeta
            builder.HasOne(t => t.Administrador)
                .WithMany()
                .HasForeignKey(t => t.AdministradorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación con consumos
            builder.HasMany(t => t.Consumos)
                .WithOne(c => c.Tarjeta)
                .HasForeignKey(c => c.TarjetaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

