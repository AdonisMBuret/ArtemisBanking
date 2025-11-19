using ArtemisBanking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBanking.Infrastructure.Data
{
    public class TarjetaCreditoConfiguration : IEntityTypeConfiguration<TarjetaCredito>
    {
        public void Configure(EntityTypeBuilder<TarjetaCredito> builder)
        {
            builder.ToTable("TarjetasCredito");

            builder.Property(t => t.NumeroTarjeta)
                .IsRequired()
                .HasMaxLength(16);

            builder.Property(t => t.FechaExpiracion)
                .IsRequired()
                .HasMaxLength(5); 
            builder.Property(t => t.CVC)
                .IsRequired()
                .HasMaxLength(256); 

            builder.Property(t => t.LimiteCredito)
                .HasColumnType("decimal(18,2)");

            builder.Property(t => t.DeudaActual)
                .HasColumnType("decimal(18,2)");

            builder.HasIndex(t => t.NumeroTarjeta)
                .IsUnique();

            builder.HasOne(t => t.Administrador)
                .WithMany()
                .HasForeignKey(t => t.AdministradorId)
                .OnDelete(DeleteBehavior.Restrict);

            
            builder.HasMany(t => t.Consumos)
                .WithOne(c => c.Tarjeta)
                .HasForeignKey(c => c.TarjetaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

