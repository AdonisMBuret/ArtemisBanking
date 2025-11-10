using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Domain.Interfaces.Repositories;
using ArtemisBanking.Infrastructure.Data;
using ArtemisBanking.Infrastructure.Jobs;
using ArtemisBanking.Application.Mappings;
using ArtemisBanking.Infrastructure.Repositories;
using ArtemisBanking.Infrastructure.Services;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Application.Services;

namespace ArtemisBanking.Infrastructure
{
    /// <summary>
    /// Clase de extensión para configurar todos los servicios de la capa de infraestructura
    /// Esto permite una configuración limpia desde Program.cs
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Configura todos los servicios de infraestructura
        /// </summary>
        public static IServiceCollection AgregarInfraestructura(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configurar DbContext con SQL Server
            services.AddDbContext<ArtemisBankingDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ArtemisBankingDbContext).Assembly.FullName)
                )
            );

            // Configurar Identity con nuestro Usuario personalizado
            services.AddIdentity<Usuario, IdentityRole>(options =>
            {
                // Configuración de contraseñas
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;

                // Configuración de usuarios
                options.User.RequireUniqueEmail = true;

                // Configuración de lockout
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;

                // Configuración de tokens
                options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultProvider;
                options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
            })
            .AddEntityFrameworkStores<ArtemisBankingDbContext>()
            .AddDefaultTokenProviders();

            // Configurar cookies de autenticación
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromHours(24);
                options.SlidingExpiration = true;
            });

            // Configurar Hangfire para jobs programados
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(
                    configuration.GetConnectionString("DefaultConnection"),
                    new SqlServerStorageOptions
                    {
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.Zero,
                        UseRecommendedIsolationLevel = true,
                        DisableGlobalLocks = true
                    }
                )
            );

            // Agregar el servidor de Hangfire
            services.AddHangfireServer();

            // Registrar repositorios
            services.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
            services.AddScoped<IRepositorioCuentaAhorro, RepositorioCuentaAhorro>();
            services.AddScoped<IRepositorioPrestamo, RepositorioPrestamo>();
            services.AddScoped<IRepositorioCuotaPrestamo, RepositorioCuotaPrestamo>();
            services.AddScoped<IRepositorioTarjetaCredito, RepositorioTarjetaCredito>();
            services.AddScoped<IRepositorioConsumoTarjeta, RepositorioConsumoTarjeta>();
            services.AddScoped<IRepositorioTransaccion, RepositorioTransaccion>();
            services.AddScoped<IRepositorioBeneficiario, RepositorioBeneficiario>();

            // Registrar servicios
            services.AddScoped<IServicioCorreo, ServicioCorreo>();
            services.AddScoped<IServicioCifrado, ServicioCifrado>();
            services.AddScoped<IServicioCalculoPrestamo, ServicioCalculoPrestamo>();

            // Registrar el job de Hangfire
            services.AddScoped<ActualizadorCuotasAtrasadasJob>();

            // Configurar AutoMapper (lo configuraremos después)
            services.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperProfile>());

            return services;
        }

        /// <summary>
        /// Configura los jobs recurrentes de Hangfire
        /// Se llama desde Program.cs después de construir la aplicación
        /// </summary>
        public static void ConfigurarJobsRecurrentes()
        {
            // Job para actualizar cuotas atrasadas - se ejecuta todos los días a las 00:01
            RecurringJob.AddOrUpdate<ActualizadorCuotasAtrasadasJob>(
                "actualizar-cuotas-atrasadas",
                job => job.EjecutarAsync(),
                Cron.Daily(0, 1) // Cada día a las 00:01
            );
        }
    }
}