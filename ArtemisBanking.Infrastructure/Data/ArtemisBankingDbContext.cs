using ArtemisBanking.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Infrastructure.Data
{
    /// <summary>
    /// Contexto de base de datos de la aplicación
    /// Es como el "puente" entre nuestro código C# y la base de datos SQL Server
    /// Hereda de IdentityDbContext para poder usar el sistema de usuarios de ASP.NET Identity
    /// </summary>
    public class ArtemisBankingDbContext : IdentityDbContext<Usuario>
    {
        // Constructor que recibe las opciones de configuración
        public ArtemisBankingDbContext(DbContextOptions<ArtemisBankingDbContext> options)// 
            : base(options)
        {
        }

        // ==================== TABLAS DE LA BASE DE DATOS ====================
        // Cada DbSet representa una tabla en la base de datos
        
        // Tabla de cuentas de ahorro
        public DbSet<CuentaAhorro> CuentasAhorro { get; set; }
        
        // Tabla de préstamos
        public DbSet<Prestamo> Prestamos { get; set; }
        
        // Tabla de cuotas de los préstamos (tabla de amortización)
        public DbSet<CuotaPrestamo> CuotasPrestamo { get; set; }
        
        // Tabla de tarjetas de crédito
        public DbSet<TarjetaCredito> TarjetasCredito { get; set; }
        
        // Tabla de consumos de tarjetas
        public DbSet<ConsumoTarjeta> ConsumosTarjeta { get; set; }
        
        // Tabla de transacciones en cuentas de ahorro
        public DbSet<Transaccion> Transacciones { get; set; }
        
        // Tabla de beneficiarios de los clientes
        public DbSet<Beneficiario> Beneficiarios { get; set; }

        /// <summary>
        /// Método que se ejecuta cuando Entity Framework está creando el modelo de la base de datos
        /// Aquí configuramos las relaciones entre tablas, índices, restricciones, etc.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Llamar al método base para que Identity configure sus tablas
            base.OnModelCreating(modelBuilder);

            // ==================== CONFIGURACIÓN DE CUENTAS DE AHORRO ====================
            modelBuilder.Entity<CuentaAhorro>(entity =>
            {
                // El número de cuenta debe ser único en toda la tabla
                entity.HasIndex(c => c.NumeroCuenta).IsUnique();
                
                // Configurar la precisión del balance (dinero)
                // 18 dígitos en total, 2 decimales
                entity.Property(c => c.Balance)
                    .HasColumnType("decimal(18,2)");
                
                // Relación: Una cuenta pertenece a UN usuario
                entity.HasOne(c => c.Usuario)
                    .WithMany(u => u.CuentasAhorro)
                    .HasForeignKey(c => c.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict); // No borrar usuario si tiene cuentas
            });

            // ==================== CONFIGURACIÓN DE PRÉSTAMOS ====================
            modelBuilder.Entity<Prestamo>(entity =>
            {
                // El número de préstamo debe ser único
                entity.HasIndex(p => p.NumeroPrestamo).IsUnique();
                
                // Configurar precisión de los campos de dinero
                entity.Property(p => p.MontoCapital)
                    .HasColumnType("decimal(18,2)");
                    
                entity.Property(p => p.TasaInteresAnual)
                    .HasColumnType("decimal(5,2)");
                    
                entity.Property(p => p.CuotaMensual)
                    .HasColumnType("decimal(18,2)");
                
                // Relación: Un préstamo pertenece a UN cliente
                entity.HasOne(p => p.Cliente)
                    .WithMany(u => u.Prestamos)
                    .HasForeignKey(p => p.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Relación: Un préstamo fue creado por UN administrador
                entity.HasOne(p => p.Administrador)
                    .WithMany()
                    .HasForeignKey(p => p.AdministradorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==================== CONFIGURACIÓN DE CUOTAS DE PRÉSTAMO ====================
            modelBuilder.Entity<CuotaPrestamo>(entity =>
            {
                // Configurar precisión del monto de la cuota
                entity.Property(c => c.MontoCuota)
                    .HasColumnType("decimal(18,2)");
                
                // Relación: Una cuota pertenece a UN préstamo
                entity.HasOne(c => c.Prestamo)
                    .WithMany(p => p.TablaAmortizacion)
                    .HasForeignKey(c => c.PrestamoId)
                    .OnDelete(DeleteBehavior.Cascade); // Si se borra el préstamo, se borran las cuotas
            });

            // ==================== CONFIGURACIÓN DE TARJETAS DE CRÉDITO ====================
            modelBuilder.Entity<TarjetaCredito>(entity =>
            {
                // El número de tarjeta debe ser único
                entity.HasIndex(t => t.NumeroTarjeta).IsUnique();
                
                // Configurar precisión de los campos de dinero
                entity.Property(t => t.LimiteCredito)
                    .HasColumnType("decimal(18,2)");
                    
                entity.Property(t => t.DeudaActual)
                    .HasColumnType("decimal(18,2)");
                
                // Relación: Una tarjeta pertenece a UN cliente
                entity.HasOne(t => t.Cliente)
                    .WithMany(u => u.TarjetasCredito)
                    .HasForeignKey(t => t.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Relación: Una tarjeta fue creada por UN administrador
                entity.HasOne(t => t.Administrador)
                    .WithMany()
                    .HasForeignKey(t => t.AdministradorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==================== CONFIGURACIÓN DE CONSUMOS DE TARJETA ====================
            modelBuilder.Entity<ConsumoTarjeta>(entity =>
            {
                // Configurar precisión del monto del consumo
                entity.Property(c => c.Monto)
                    .HasColumnType("decimal(18,2)");
                
                // Relación: Un consumo pertenece a UNA tarjeta
                entity.HasOne(c => c.Tarjeta)
                    .WithMany(t => t.Consumos)
                    .HasForeignKey(c => c.TarjetaId)
                    .OnDelete(DeleteBehavior.Cascade); // Si se borra la tarjeta, se borran los consumos
            });

            // ==================== CONFIGURACIÓN DE TRANSACCIONES ====================
            modelBuilder.Entity<Transaccion>(entity =>
            {
                // Configurar precisión del monto de la transacción
                entity.Property(t => t.Monto)
                    .HasColumnType("decimal(18,2)");
                
                // Relación: Una transacción pertenece a UNA cuenta de ahorro
                entity.HasOne(t => t.CuentaAhorro)
                    .WithMany(c => c.Transacciones)
                    .HasForeignKey(t => t.CuentaAhorroId)
                    .OnDelete(DeleteBehavior.Cascade); // Si se borra la cuenta, se borran las transacciones
            });

            // ==================== CONFIGURACIÓN DE BENEFICIARIOS ====================
            modelBuilder.Entity<Beneficiario>(entity =>
            {
                // Relación: Un beneficiario pertenece a UN usuario
                entity.HasOne(b => b.Usuario)
                    .WithMany(u => u.Beneficiarios)
                    .HasForeignKey(b => b.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade); // Si se borra el usuario, se borran sus beneficiarios
                
                // Relación: Un beneficiario referencia a UNA cuenta de ahorro
                entity.HasOne(b => b.CuentaAhorro)
                    .WithMany()
                    .HasForeignKey(b => b.CuentaAhorroId)
                    .OnDelete(DeleteBehavior.Restrict); // No borrar la cuenta si es beneficiaria de alguien
            });
        }
    }
}