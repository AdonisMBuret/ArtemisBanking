using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Application.DTOs.Api;
using ArtemisBanking.Application.ViewModels;
using ArtemisBanking.Application.ViewModels.Usuario;
using ArtemisBanking.Application.ViewModels.Cliente;
using ArtemisBanking.Application.ViewModels.Comercio;
using ArtemisBanking.Application.ViewModels.Prestamo;
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

            // ==================== MAPEOS DE VIEWMODEL <-> DTO  ====================
                        
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

            // DTO → ViewModel para Cliente
            CreateMap<CuentaAhorroDTO, CuentaClienteViewModel>()
                .ForMember(dest => dest.TipoCuenta,
                    opt => opt.MapFrom(src => src.EsPrincipal ? "Principal" : "Secundaria"));

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

            // DTO → ViewModel para Cliente
            CreateMap<PrestamoDTO, PrestamoClienteViewModel>()
                .ForMember(dest => dest.Estado,
                    opt => opt.MapFrom(src => src.EstaAlDia ? "Al día" : "En mora"));

            // ==================== MAPEO DE TARJETA DE CRÉDITO ====================
            CreateMap<TarjetaCredito, TarjetaCreditoDTO>()
                .ForMember(dest => dest.NombreCliente,
                    opt => opt.MapFrom(src => src.Cliente.Nombre))
                .ForMember(dest => dest.ApellidoCliente,
                    opt => opt.MapFrom(src => src.Cliente.Apellido))
                .ForMember(dest => dest.CedulaCliente,
                    opt => opt.MapFrom(src => src.Cliente.Cedula));

            // DTO → ViewModel para Cliente
            CreateMap<TarjetaCreditoDTO, TarjetaClienteViewModel>();

            // ==================== MAPEO DE TRANSACCIÓN ====================
            CreateMap<Transaccion, TransaccionDTO>()
                .ForMember(dest => dest.NumeroCuenta,
                    opt => opt.MapFrom(src => src.CuentaAhorro.NumeroCuenta));

            // DTO → ViewModel para Cliente
            CreateMap<TransaccionDTO, TransaccionClienteViewModel>();

            // ==================== MAPEO DE CONSUMO DE TARJETA ====================
            CreateMap<ConsumoTarjeta, ConsumoTarjetaDTO>()
                .ForMember(dest => dest.NumeroTarjeta,
                    opt => opt.MapFrom(src => src.Tarjeta.NumeroTarjeta));

            // DTO → ViewModel para Cliente
            CreateMap<ConsumoTarjetaDTO, ConsumoTarjetaClienteViewModel>();

            // ==================== MAPEO DE CUOTA DE PRÉSTAMO ====================
            CreateMap<CuotaPrestamo, CuotaPrestamoDTO>();

            // DTO → ViewModel
            CreateMap<CuotaPrestamoDTO, CuotaPrestamoViewModel>();

            // ==================== MAPEO DE BENEFICIARIO ====================
            CreateMap<Beneficiario, BeneficiarioDTO>()
                .ForMember(dest => dest.NombreBeneficiario,
                    opt => opt.MapFrom(src => src.CuentaAhorro.Usuario.Nombre))
                .ForMember(dest => dest.ApellidoBeneficiario,
                    opt => opt.MapFrom(src => src.CuentaAhorro.Usuario.Apellido));

            // DTO → ViewModel para Cliente
            CreateMap<BeneficiarioDTO, BeneficiarioItemViewModel>();

            // ==================== MAPEOS PARA DASHBOARD CLIENTE ====================
            CreateMap<DashboardClienteDTO, HomeClienteViewModel>()
                .ForMember(dest => dest.CuentasAhorro,
                    opt => opt.MapFrom(src => src.CuentasAhorro))
                .ForMember(dest => dest.Prestamos,
                    opt => opt.MapFrom(src => src.Prestamos))
                .ForMember(dest => dest.TarjetasCredito,
                    opt => opt.MapFrom(src => src.TarjetasCredito));

            CreateMap<DetalleCuentaClienteDTO, DetalleCuentaClienteViewModel>()
                .ForMember(dest => dest.TipoCuenta,
                    opt => opt.MapFrom(src => src.EsPrincipal ? "Principal" : "Secundaria"));

            CreateMap<DetallePrestamoClienteDTO, DetallePrestamoClienteViewModel>();

            CreateMap<DetalleTarjetaClienteDTO, DetalleTarjetaClienteViewModel>();

            // ==================== MAPEOS DE COMERCIO ====================
            CreateMap<ComercioResponseDTO, ComercioItemViewModel>()
                .ForMember(dest => dest.TieneUsuario,
                    opt => opt.MapFrom(src => src.Usuario != null));

            CreateMap<ComercioResponseDTO, DetalleComercioViewModel>()
                .ForMember(dest => dest.TieneUsuario,
                    opt => opt.MapFrom(src => src.Usuario != null))
                .ForMember(dest => dest.TotalConsumos,
                    opt => opt.Ignore()) // Se calculará en el servicio
                .ForMember(dest => dest.MontoTotalConsumos,
                    opt => opt.Ignore()); // Se calculará en el servicio

            CreateMap<ComercioResponseDTO, EditarComercioViewModel>();

            CreateMap<CrearComercioViewModel, CrearComercioRequestDTO>();
            CreateMap<EditarComercioViewModel, ActualizarComercioRequestDTO>();
        }
    }
}