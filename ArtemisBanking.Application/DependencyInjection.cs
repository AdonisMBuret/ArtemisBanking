using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Application.Mappings;
using ArtemisBanking.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ArtemisBanking.Application
{
    /// <summary>
    /// Clase de extensión para configurar los servicios de la capa de aplicación
    /// Aquí registramos todos los servicios que contienen la lógica de negocio
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Método que agrega todos los servicios de aplicación al contenedor de DI
        /// Se llama desde Program.cs en la capa Web
        /// </summary>
        public static IServiceCollection AgregarAplicacion(this IServiceCollection services)
        {
            // ==================== AUTOMAPPER ====================
            // Configurar AutoMapper con nuestro perfil de mapeos
            services.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperProfile>());

            // ==================== SERVICIOS DE NEGOCIO ====================
            // Estos servicios contienen toda la lógica de negocio de la aplicación

            // Servicio para cálculos de préstamos (sistema francés)
            services.AddScoped<IServicioCalculoPrestamo, ServicioCalculoPrestamo>();

            // Servicio para gestión de usuarios (crear, editar, dashboard)
            services.AddScoped<IServicioUsuario, ServicioUsuario>();

            // Servicio para gestión de préstamos (asignar, actualizar tasa, etc.)
            services.AddScoped<IServicioPrestamo, ServicioPrestamo>();

            // Servicio para gestión de tarjetas de crédito
            services.AddScoped<IServicioTarjetaCredito, ServicioTarjetaCredito>();

            // Servicio para gestión de cuentas de ahorro
            services.AddScoped<IServicioCuentaAhorro, ServicioCuentaAhorro>();

            // Servicio para transacciones (transferencias, pagos, avances)
            services.AddScoped<IServicioTransaccion, ServicioTransaccion>();

            // Servicio para gestión de beneficiarios
            services.AddScoped<IServicioBeneficiario, ServicioBeneficiario>();

            // Servicio para operaciones de cajero
            services.AddScoped<IServicioCajero, ServicioCajero>();

            return services;
        }
    }
}