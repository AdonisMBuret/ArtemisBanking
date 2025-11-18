using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Application.Mappings;
using ArtemisBanking.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ArtemisBanking.Application
{
     
    /// Clase de extensión para configurar los servicios de la capa de aplicación
    /// Aquí registramos todos los servicios que contienen la lógica de negocio
     
    public static class DependencyInjection
    {
         
        /// Método que agrega todos los servicios de aplicación al contenedor de DI
        /// Se llama desde Program.cs en la capa Web
         
        public static IServiceCollection AgregarAplicacion(this IServiceCollection services)
        {
            // ==================== AUTOMAPPER ====================
            services.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperProfile>());

            // ==================== SERVICIOS DE NEGOCIO ====================

            // Servicio de autenticación
            services.AddScoped<IServicioAutenticacion, ServicioAutenticacion>();

            // Servicio para cálculos de préstamos
            services.AddScoped<IServicioCalculoPrestamo, ServicioCalculoPrestamo>();

            // Servicio para gestión de usuarios
            services.AddScoped<IServicioUsuario, ServicioUsuario>();

            // Servicio para gestión de préstamos
            services.AddScoped<IServicioPrestamo, ServicioPrestamo>();

            // Servicio para gestión de tarjetas
            services.AddScoped<IServicioTarjetaCredito, ServicioTarjetaCredito>();

            // Servicio para gestión de cuentas
            services.AddScoped<IServicioCuentaAhorro, ServicioCuentaAhorro>();

            // Servicio para transacciones
            services.AddScoped<IServicioTransaccion, ServicioTransaccion>();

            // Servicio para beneficiarios
            services.AddScoped<IServicioBeneficiario, ServicioBeneficiario>();

            // Servicio para operaciones de cajero
            services.AddScoped<IServicioCajero, ServicioCajero>();

            // Servicio para gestión de comercios
            services.AddScoped<IServicioComercio, ServicioComercio>();

            // Servicio para procesamiento de pagos (Hermes Pay)
            services.AddScoped<IServicioProcesadorPagos, ServicioProcesadorPagos>();

            // Servicio para dashboard del cliente
            services.AddScoped<IServicioDashboardCliente, ServicioDashboardCliente>();

            // ⭐ NUEVO - Servicio para consumos con tarjeta (comercios)
            services.AddScoped<IServicioConsumoTarjeta, ServicioConsumoTarjeta>();

            return services;
        }
    }
}