using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Application.Mappings;
using ArtemisBanking.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ArtemisBanking.Application
{
        
    public static class DependencyInjection
    {
         
        public static IServiceCollection AgregarAplicacion(this IServiceCollection services)
        {
            // AUTOMAPPER 
            services.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperProfile>());

            // SERVICIOS DE NEGOCIO 

            services.AddScoped<IServicioAutenticacion, ServicioAutenticacion>();

            services.AddScoped<IServicioCalculoPrestamo, ServicioCalculoPrestamo>();

            services.AddScoped<IServicioUsuario, ServicioUsuario>();

            services.AddScoped<IServicioPrestamo, ServicioPrestamo>();

            services.AddScoped<IServicioTarjetaCredito, ServicioTarjetaCredito>();

            services.AddScoped<IServicioCuentaAhorro, ServicioCuentaAhorro>();

            services.AddScoped<IServicioTransaccion, ServicioTransaccion>();

            services.AddScoped<IServicioBeneficiario, ServicioBeneficiario>();

            services.AddScoped<IServicioCajero, ServicioCajero>();

            services.AddScoped<IServicioComercio, ServicioComercio>();

            services.AddScoped<IServicioProcesadorPagos, ServicioProcesadorPagos>();

            services.AddScoped<IServicioDashboardCliente, ServicioDashboardCliente>();

            services.AddScoped<IServicioConsumoTarjeta, ServicioConsumoTarjeta>();

            return services;
        }
    }
}