using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Application.ViewModels;
using ArtemisBanking.Application.ViewModels.Usuario;
using ArtemisBanking.Domain.Entities;
using AutoMapper;

namespace ArtemisBanking.Application.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // ==================== MAPEO DE USUARIO ====================
            CreateMap<Usuario, UsuarioDTO>()
                .ForMember(dest => dest.NombreCompleto,
                    opt => opt.MapFrom(src => $"{src.Nombre} {src.Apellido}"))
                .ForMember(dest => dest.Correo,
                    opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.NombreUsuario,
                    opt => opt.MapFrom(src => src.UserName));

            // ==================== MAPEOS DE VIEWMODEL <-> DTO (¡AQUÍ!) ====================

            
            CreateMap<DashboardCajeroDTO, DashboardCajeroViewModel>();

            CreateMap<CrearUsuarioViewModel, CrearUsuarioDTO>();

            CreateMap<EditarUsuarioViewModel, ActualizarUsuarioDTO>();

            // ==================== MAPEO DE CUENTA DE AHORRO ====================
            CreateMap<CuentaAhorro, CuentaAhorroDTO>()
                .ForMember(dest => dest.NombreCliente,
                    opt => opt.MapFrom(src => src.Usuario.Nombre))
                .ForMember(dest => dest.ApellidoCliente,
                    opt => opt.MapFrom(src => src.Usuario.Apellido))
                .ForMember(dest => dest.CedulaCliente,
                    opt => opt.MapFrom(src => src.Usuario.Cedula));

            // ==================== MAPEO DE PRÉSTAMO ====================
            CreateMap<Prestamo, PrestamoDTO>()
                .ForMember(dest => dest.NombreCliente,
                    opt => opt.MapFrom(src => src.Cliente.Nombre))
                .ForMember(dest => dest.ApellidoCliente,
                    opt => opt.MapFrom(src => src.Cliente.Apellido))
                .ForMember(dest => dest.CedulaCliente,
                    opt => opt.MapFrom(src => src.Cliente.Cedula))
                .ForMember(dest => dest.TotalCuotas,
                    opt => opt.MapFrom(src => src.PlazoMeses))
                .ForMember(dest => dest.MontoPendiente,
                    opt => opt.MapFrom(src => src.TablaAmortizacion.Where(c => !c.EstaPagada).Sum(c => c.MontoCuota)));

            // ==================== MAPEO DE TARJETA DE CRÉDITO ====================
            CreateMap<TarjetaCredito, TarjetaCreditoDTO>()
                .ForMember(dest => dest.NombreCliente,
                    opt => opt.MapFrom(src => src.Cliente.Nombre))
                .ForMember(dest => dest.ApellidoCliente,
                    opt => opt.MapFrom(src => src.Cliente.Apellido))
                .ForMember(dest => dest.CedulaCliente,
                    opt => opt.MapFrom(src => src.Cliente.Cedula));

            // ==================== MAPEO DE TRANSACCIÓN ====================
            CreateMap<Transaccion, TransaccionDTO>()
                .ForMember(dest => dest.NumeroCuenta,
                    opt => opt.MapFrom(src => src.CuentaAhorro.NumeroCuenta));

            // ==================== MAPEO DE CONSUMO DE TARJETA ====================
            CreateMap<ConsumoTarjeta, ConsumoTarjetaDTO>()
                .ForMember(dest => dest.NumeroTarjeta,
                    opt => opt.MapFrom(src => src.Tarjeta.NumeroTarjeta));

            // ==================== MAPEO DE CUOTA DE PRÉSTAMO ====================
            CreateMap<CuotaPrestamo, CuotaPrestamoDTO>();

            // ==================== MAPEO DE BENEFICIARIO ====================
            CreateMap<Beneficiario, BeneficiarioDTO>()
                .ForMember(dest => dest.NombreBeneficiario,
                    opt => opt.MapFrom(src => src.CuentaAhorro.Usuario.Nombre))
                .ForMember(dest => dest.ApellidoBeneficiario,
                    opt => opt.MapFrom(src => src.CuentaAhorro.Usuario.Apellido));

            // ==================== MAPEOS DE DTOs (Application layer) ====================
            // Estos son válidos porque ambos están en la misma capa
            CreateMap<CrearUsuarioDTO, Usuario>();
            CreateMap<ActualizarUsuarioDTO, Usuario>();
        }
    }
}