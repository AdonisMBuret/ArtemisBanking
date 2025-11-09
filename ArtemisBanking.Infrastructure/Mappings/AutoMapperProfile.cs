using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Domain.Entities;
using AutoMapper;

namespace ArtemisBanking.Infrastructure.Mappings
{
    /// <summary>
    /// Perfil de AutoMapper para mapeo entre entidades y DTOs
    /// Define cómo se convierten los objetos de un tipo a otro
    /// </summary>
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Mapeo de Usuario a UsuarioDTO
            CreateMap<Usuario, UsuarioDTO>()
                .ForMember(dest => dest.NombreCompleto,
                          opt => opt.MapFrom(src => $"{src.Nombre} {src.Apellido}"))
                .ForMember(dest => dest.Correo,
                          opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.NombreUsuario,
                          opt => opt.MapFrom(src => src.UserName));

            // Mapeo de CuentaAhorro a CuentaAhorroDTO
            CreateMap<CuentaAhorro, CuentaAhorroDTO>()
                .ForMember(dest => dest.NombreCliente,
                          opt => opt.MapFrom(src => src.Usuario.Nombre))
                .ForMember(dest => dest.ApellidoCliente,
                          opt => opt.MapFrom(src => src.Usuario.Apellido))
                .ForMember(dest => dest.CedulaCliente,
                          opt => opt.MapFrom(src => src.Usuario.Cedula));

            // Mapeo de Prestamo a PrestamoDTO
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
                          opt => opt.MapFrom(src =>
                              src.TablaAmortizacion.Where(c => !c.EstaPagada).Sum(c => c.MontoCuota)));

            // Mapeo de TarjetaCredito a TarjetaCreditoDTO
            CreateMap<TarjetaCredito, TarjetaCreditoDTO>()
                .ForMember(dest => dest.NombreCliente,
                          opt => opt.MapFrom(src => src.Cliente.Nombre))
                .ForMember(dest => dest.ApellidoCliente,
                          opt => opt.MapFrom(src => src.Cliente.Apellido))
                .ForMember(dest => dest.CedulaCliente,
                          opt => opt.MapFrom(src => src.Cliente.Cedula));

            // Mapeo de Transaccion a TransaccionDTO
            CreateMap<Transaccion, TransaccionDTO>()
                .ForMember(dest => dest.NumeroCuenta,
                          opt => opt.MapFrom(src => src.CuentaAhorro.NumeroCuenta));

            // Mapeo de ConsumoTarjeta a ConsumoTarjetaDTO
            CreateMap<ConsumoTarjeta, ConsumoTarjetaDTO>()
                .ForMember(dest => dest.NumeroTarjeta,
                          opt => opt.MapFrom(src => src.Tarjeta.NumeroTarjeta));

            // Mapeo de CuotaPrestamo a CuotaPrestamoDTO
            CreateMap<CuotaPrestamo, CuotaPrestamoDTO>();

            // Mapeo de Beneficiario a BeneficiarioDTO
            CreateMap<Beneficiario, BeneficiarioDTO>()
                .ForMember(dest => dest.NombreBeneficiario,
                          opt => opt.MapFrom(src => src.CuentaAhorro.Usuario.Nombre))
                .ForMember(dest => dest.ApellidoBeneficiario,
                          opt => opt.MapFrom(src => src.CuentaAhorro.Usuario.Apellido));
        }
    }
}