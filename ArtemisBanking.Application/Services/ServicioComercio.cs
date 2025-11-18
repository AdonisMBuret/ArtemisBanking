using ArtemisBanking.Application.DTOs.Api;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Domain.Interfaces.Repositories;
using AutoMapper;

namespace ArtemisBanking.Application.Services
{
    public class ServicioComercio : IServicioComercio
    {
        private readonly IRepositorioComercio _repositorioComercio;
        private readonly IRepositorioUsuario _repositorioUsuario;
        private readonly IMapper _mapper;

        public ServicioComercio(
            IRepositorioComercio repositorioComercio,
            IRepositorioUsuario repositorioUsuario,
            IMapper mapper)
        {
            _repositorioComercio = repositorioComercio;
            _repositorioUsuario = repositorioUsuario;
            _mapper = mapper;
        }

        public async Task<PaginatedResponseDTO<ComercioResponseDTO>> ObtenerComerciosPaginadosAsync(int page = 1, int pageSize = 20)
        {
            var (comercios, totalRegistros) = await _repositorioComercio.ObtenerPaginadoAsync(page, pageSize);

            var comerciosDTO = comercios.Select(c => new ComercioResponseDTO
            {
                Id = c.Id,
                Nombre = c.Nombre,
                RNC = c.RNC,
                EstaActivo = c.EstaActivo,
                FechaCreacion = c.FechaCreacion,
                Usuario = c.Usuario != null ? new UsuarioComercioDTO
                {
                    Id = c.Usuario.Id,
                    UserName = c.Usuario.UserName ?? string.Empty,
                    Email = c.Usuario.Email ?? string.Empty,
                    NombreCompleto = c.Usuario.NombreCompleto,
                    EstaActivo = c.Usuario.EstaActivo
                } : null
            });

            return new PaginatedResponseDTO<ComercioResponseDTO>
            {
                Data = comerciosDTO,
                Page = page,
                PageSize = pageSize,
                TotalRecords = totalRegistros
            };
        }

        public async Task<IEnumerable<ComercioResponseDTO>> ObtenerComerciosActivosAsync()
        {
            var comercios = await _repositorioComercio.ObtenerActivosAsync();

            return comercios.Select(c => new ComercioResponseDTO
            {
                Id = c.Id,
                Nombre = c.Nombre,
                RNC = c.RNC,
                EstaActivo = c.EstaActivo,
                FechaCreacion = c.FechaCreacion,
                Usuario = c.Usuario != null ? new UsuarioComercioDTO
                {
                    Id = c.Usuario.Id,
                    UserName = c.Usuario.UserName ?? string.Empty,
                    Email = c.Usuario.Email ?? string.Empty,
                    NombreCompleto = c.Usuario.NombreCompleto,
                    EstaActivo = c.Usuario.EstaActivo
                } : null
            });
        }

        public async Task<ComercioResponseDTO?> ObtenerComercioPorIdAsync(int id)
        {
            var comercio = await _repositorioComercio.ObtenerConUsuarioAsync(id);

            if (comercio == null)
                return null;

            return new ComercioResponseDTO
            {
                Id = comercio.Id,
                Nombre = comercio.Nombre,
                RNC = comercio.RNC,
                EstaActivo = comercio.EstaActivo,
                FechaCreacion = comercio.FechaCreacion,
                Usuario = comercio.Usuario != null ? new UsuarioComercioDTO
                {
                    Id = comercio.Usuario.Id,
                    UserName = comercio.Usuario.UserName ?? string.Empty,
                    Email = comercio.Usuario.Email ?? string.Empty,
                    NombreCompleto = comercio.Usuario.NombreCompleto,
                    EstaActivo = comercio.Usuario.EstaActivo
                } : null
            };
        }

        public async Task<ComercioResponseDTO> CrearComercioAsync(CrearComercioRequestDTO request)
        {
            // Verificar que el RNC no exista
            var comercioExistente = await _repositorioComercio.ObtenerPorRNCAsync(request.RNC);
            if (comercioExistente != null)
            {
                throw new InvalidOperationException("Ya existe un comercio con este RNC");
            }

            var nuevoComercio = new Comercio
            {
                Nombre = request.Nombre,
                RNC = request.RNC,
                EstaActivo = true,
                FechaCreacion = DateTime.Now
            };

            await _repositorioComercio.AgregarAsync(nuevoComercio);
            await _repositorioComercio.GuardarCambiosAsync();

            return new ComercioResponseDTO
            {
                Id = nuevoComercio.Id,
                Nombre = nuevoComercio.Nombre,
                RNC = nuevoComercio.RNC,
                EstaActivo = nuevoComercio.EstaActivo,
                FechaCreacion = nuevoComercio.FechaCreacion
            };
        }

        public async Task<bool> ActualizarComercioAsync(int id, ActualizarComercioRequestDTO request)
        {
            var comercio = await _repositorioComercio.ObtenerPorIdAsync(id);
            if (comercio == null)
                return false;

            // Verificar que el RNC no esté en uso por otro comercio
            var comercioConRNC = await _repositorioComercio.ObtenerPorRNCAsync(request.RNC);
            if (comercioConRNC != null && comercioConRNC.Id != id)
            {
                throw new InvalidOperationException("El RNC ya está en uso por otro comercio");
            }

            comercio.Nombre = request.Nombre;
            comercio.RNC = request.RNC;

            await _repositorioComercio.ActualizarAsync(comercio);
            await _repositorioComercio.GuardarCambiosAsync();

            return true;
        }

        public async Task<bool> CambiarEstadoComercioAsync(int id, bool nuevoEstado)
        {
            var comercio = await _repositorioComercio.ObtenerConUsuarioAsync(id);
            if (comercio == null)
                return false;

            comercio.EstaActivo = nuevoEstado;

            // Si se desactiva el comercio, desactivar también su usuario
            if (!nuevoEstado && comercio.Usuario != null)
            {
                comercio.Usuario.EstaActivo = false;
                // El usuario se actualizará en cascada con el comercio
            }

            await _repositorioComercio.ActualizarAsync(comercio);
            await _repositorioComercio.GuardarCambiosAsync();

            return true;
        }

        public async Task<bool> TieneUsuarioAsociadoAsync(int comercioId)
        {
            return await _repositorioComercio.TieneUsuarioAsociadoAsync(comercioId);
        }
    }
}
