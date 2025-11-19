using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Domain.Interfaces.Repositories;
using ArtemisBanking.Infrastructure.Data;
using ArtemisBanking.Infrastructure.Jobs;
using ArtemisBanking.Infrastructure.Repositories;
using ArtemisBanking.Infrastructure.Services;
using ArtemisBanking.Application.Interfaces;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArtemisBanking.Infrastructure
{
     
    // Clase de extensión para configurar todos los servicios de infraestructura
     
    public static class DependencyInjection
    {      
        public static IServiceCollection AgregarInfraestructura(
            this IServiceCollection services,
            IConfiguration configuration,
            bool configurarCookies = true)
        {
            // BASE DE DATOS 
            services.AddDbContext<ArtemisBankingDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ArtemisBankingDbContext).Assembly.FullName)
                )
            );

            // IDENTITY 
            services.AddIdentity<Usuario, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;

                options.User.RequireUniqueEmail = true;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;

                options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultProvider;
                options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
            })
            .AddEntityFrameworkStores<ArtemisBankingDbContext>()
            .AddDefaultTokenProviders();

            // COOKIES DE AUTENTICACIÓN 
            // Solo configurar cookies para Razor Pages, no para APIs con JWT
            if (configurarCookies)
            {
                services.ConfigureApplicationCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(24);
                    options.SlidingExpiration = true;
                });
            }

            // HANGFIRE 
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

            services.AddHangfireServer();

            // REPOSITORIOS 
            services.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
            services.AddScoped<IRepositorioCuentaAhorro, RepositorioCuentaAhorro>();
            services.AddScoped<IRepositorioPrestamo, RepositorioPrestamo>();
            services.AddScoped<IRepositorioCuotaPrestamo, RepositorioCuotaPrestamo>();
            services.AddScoped<IRepositorioTarjetaCredito, RepositorioTarjetaCredito>();
            services.AddScoped<IRepositorioConsumoTarjeta, RepositorioConsumoTarjeta>();
            services.AddScoped<IRepositorioTransaccion, RepositorioTransaccion>();
            services.AddScoped<IRepositorioBeneficiario, RepositorioBeneficiario>();
            services.AddScoped<IRepositorioComercio, RepositorioComercio>();

            // SERVICIOS DE INFRAESTRUCTURA 

            // Servicio para envío de correos (SMTP)
            services.AddScoped<IServicioCorreo, ServicioCorreo>();

            // Servicio para cifrado de datos (SHA-256)
            services.AddScoped<IServicioCifrado, ServicioCifrado>();

            // Servicio para generación y validación de JWT
            services.AddScoped<IServicioJwt, ServicioJwt>();

            // JOBS DE HANGFIRE 
            services.AddScoped<ActualizadorCuotasAtrasadasJob>();

            return services;
        }
         
        // Configura los jobs recurrentes de Hangfire         
        public static void ConfigurarJobsRecurrentes()
        {
            // Job para actualizar cuotas atrasadas - se ejecuta todos los días a las 00:01
            RecurringJob.AddOrUpdate<ActualizadorCuotasAtrasadasJob>(
                "actualizar-cuotas-atrasadas",
                job => job.EjecutarAsync(),
                Cron.Daily(0, 1) 
            );
        }
    }
}