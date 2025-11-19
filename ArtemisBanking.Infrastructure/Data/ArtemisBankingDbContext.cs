using ArtemisBanking.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Infrastructure.Data
{
    public class ArtemisBankingDbContext : IdentityDbContext<Usuario>
    {
        
        public ArtemisBankingDbContext(DbContextOptions<ArtemisBankingDbContext> options)
            : base(options)
        {
        }

        //  TABLAS DE LA BASE DE DATOS 
   
        public DbSet<CuentaAhorro> CuentasAhorro { get; set; }
        
        public DbSet<Prestamo> Prestamos { get; set; }
        
        public DbSet<CuotaPrestamo> CuotasPrestamo { get; set; }
        
        public DbSet<TarjetaCredito> TarjetasCredito { get; set; }
        
        public DbSet<ConsumoTarjeta> ConsumosTarjeta { get; set; }
        
        public DbSet<Transaccion> Transacciones { get; set; }
        
        public DbSet<Beneficiario> Beneficiarios { get; set; }

        public DbSet<Comercio> Comercios { get; set; }

         protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // CONFIGURACIÓN DE COMERCIOS 
            modelBuilder.Entity<Comercio>(entity =>
            {
                entity.HasIndex(c => c.RNC).IsUnique();
                
                entity.Property(c => c.Nombre)
                    .IsRequired()
                    .HasMaxLength(200);
                
                entity.Property(c => c.RNC)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.HasOne(c => c.Usuario)
                    .WithOne(u => u.Comercio)
                    .HasForeignKey<Comercio>(c => c.UsuarioId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(c => c.Consumos)
                    .WithOne(ct => ct.Comercio)
                    .HasForeignKey(ct => ct.ComercioId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // CONFIGURACIÓN DE CUENTAS DE AHORRO 
            modelBuilder.Entity<CuentaAhorro>(entity =>
            {
                entity.HasIndex(c => c.NumeroCuenta).IsUnique();
                

                entity.Property(c => c.Balance)
                    .HasColumnType("decimal(18,2)");
                
                entity.HasOne(c => c.Usuario)
                    .WithMany(u => u.CuentasAhorro)
                    .HasForeignKey(c => c.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict); 
            });

            // CONFIGURACIÓN DE PRÉSTAMOS 
            modelBuilder.Entity<Prestamo>(entity =>
            {
                entity.HasIndex(p => p.NumeroPrestamo).IsUnique();
                
                entity.Property(p => p.MontoCapital)
                    .HasColumnType("decimal(18,2)");
                    
                entity.Property(p => p.TasaInteresAnual)
                    .HasColumnType("decimal(5,2)");
                    
                entity.Property(p => p.CuotaMensual)
                    .HasColumnType("decimal(18,2)");
                
                entity.HasOne(p => p.Cliente)
                    .WithMany(u => u.Prestamos)
                    .HasForeignKey(p => p.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(p => p.Administrador)
                    .WithMany()
                    .HasForeignKey(p => p.AdministradorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // CONFIGURACIÓN DE CUOTAS DE PRÉSTAMO 
            modelBuilder.Entity<CuotaPrestamo>(entity =>
            {
                entity.Property(c => c.MontoCuota)
                    .HasColumnType("decimal(18,2)");
                
                entity.HasOne(c => c.Prestamo)
                    .WithMany(p => p.TablaAmortizacion)
                    .HasForeignKey(c => c.PrestamoId)
                    .OnDelete(DeleteBehavior.Cascade); 
            });

            //  CONFIGURACIÓN DE TARJETAS DE CRÉDITO 
            modelBuilder.Entity<TarjetaCredito>(entity =>
            {
                entity.HasIndex(t => t.NumeroTarjeta).IsUnique();
                
                entity.Property(t => t.LimiteCredito)
                    .HasColumnType("decimal(18,2)");
                    
                entity.Property(t => t.DeudaActual)
                    .HasColumnType("decimal(18,2)");
                
                entity.HasOne(t => t.Cliente)
                    .WithMany(u => u.TarjetasCredito)
                    .HasForeignKey(t => t.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(t => t.Administrador)
                    .WithMany()
                    .HasForeignKey(t => t.AdministradorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // CONFIGURACIÓN DE CONSUMOS DE TARJETA 
            modelBuilder.Entity<ConsumoTarjeta>(entity =>
            {
                entity.Property(c => c.Monto)
                    .HasColumnType("decimal(18,2)");
                
                entity.HasOne(c => c.Tarjeta)
                    .WithMany(t => t.Consumos)
                    .HasForeignKey(c => c.TarjetaId)
                    .OnDelete(DeleteBehavior.Cascade); 

                entity.HasOne(c => c.Comercio)
                    .WithMany(co => co.Consumos)
                    .HasForeignKey(c => c.ComercioId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // CONFIGURACIÓN DE TRANSACCIONES 
            modelBuilder.Entity<Transaccion>(entity =>
            {
                entity.Property(t => t.Monto)
                    .HasColumnType("decimal(18,2)");
                
                entity.HasOne(t => t.CuentaAhorro)
                    .WithMany(c => c.Transacciones)
                    .HasForeignKey(t => t.CuentaAhorroId)
                    .OnDelete(DeleteBehavior.Cascade); 
            });

            // CONFIGURACIÓN DE BENEFICIARIOS 
            modelBuilder.Entity<Beneficiario>(entity =>
            {
                entity.HasOne(b => b.Usuario)
                    .WithMany(u => u.Beneficiarios)
                    .HasForeignKey(b => b.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade); 
                
                entity.HasOne(b => b.CuentaAhorro)
                    .WithMany()
                    .HasForeignKey(b => b.CuentaAhorroId)
                    .OnDelete(DeleteBehavior.Restrict); 
            });
        }
    }
}