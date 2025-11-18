using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Domain.Interfaces.Repositories;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace ArtemisBanking.Application.Services
{
    /// <summary>
    /// Servicio genérico base que implementa operaciones CRUD comunes
    /// Promueve la reutilización de código y reduce duplicación
    /// </summary>
    public abstract class ServicioGenerico<TDto, TEntidad> : IServicioGenerico<TDto, TEntidad>
        where TDto : class
        where TEntidad : class
    {
        protected readonly IRepositorioGenerico<TEntidad> _repositorio;
        protected readonly IMapper _mapper;
        protected readonly ILogger _logger;

        protected ServicioGenerico(
            IRepositorioGenerico<TEntidad> repositorio,
            IMapper mapper,
            ILogger logger)
        {
            _repositorio = repositorio;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los registros y los mapea a DTOs
        /// </summary>
        public virtual async Task<ResultadoOperacion<IEnumerable<TDto>>> ObtenerTodosAsync()
        {
            try
            {
                var entidades = await _repositorio.ObtenerTodosAsync();
                var dtos = _mapper.Map<IEnumerable<TDto>>(entidades);
                return ResultadoOperacion<IEnumerable<TDto>>.Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ups, algo salió mal al obtener todos los registros de {typeof(TEntidad).Name}");
                return ResultadoOperacion<IEnumerable<TDto>>.Fallo(
                    "Oops, no pudimos cargar los datos. Intenta de nuevo.");
            }
        }

        /// <summary>
        /// Obtiene un registro por ID y lo mapea a DTO
        /// </summary>
        public virtual async Task<ResultadoOperacion<TDto>> ObtenerPorIdAsync(int id)
        {
            try
            {
                var entidad = await _repositorio.ObtenerPorIdAsync(id);
                
                if (entidad == null)
                {
                    return ResultadoOperacion<TDto>.Fallo(
                        "No encontramos lo que buscas. Verifica que el ID sea correcto.");
                }

                var dto = _mapper.Map<TDto>(entidad);
                return ResultadoOperacion<TDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener {typeof(TEntidad).Name} con ID: {id}");
                return ResultadoOperacion<TDto>.Fallo(
                    "Hubo un problema al buscar el registro. Intenta nuevamente.");
            }
        }

        /// <summary>
        /// Obtiene registros filtrados por condición
        /// </summary>
        public virtual async Task<ResultadoOperacion<IEnumerable<TDto>>> ObtenerPorCondicionAsync(
            Expression<Func<TEntidad, bool>> filtro)
        {
            try
            {
                var entidades = await _repositorio.ObtenerPorCondicionAsync(filtro);
                var dtos = _mapper.Map<IEnumerable<TDto>>(entidades);
                return ResultadoOperacion<IEnumerable<TDto>>.Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al filtrar {typeof(TEntidad).Name}");
                return ResultadoOperacion<IEnumerable<TDto>>.Fallo(
                    "No pudimos aplicar los filtros. Intenta de nuevo.");
            }
        }

        /// <summary>
        /// Verifica si existe un registro con la condición especificada
        /// </summary>
        public virtual async Task<ResultadoOperacion<bool>> ExisteAsync(
            Expression<Func<TEntidad, bool>> filtro)
        {
            try
            {
                var existe = await _repositorio.ExisteAsync(filtro);
                return ResultadoOperacion<bool>.Ok(existe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al verificar existencia de {typeof(TEntidad).Name}");
                return ResultadoOperacion<bool>.Fallo(
                    "No pudimos verificar la información. Intenta más tarde.");
            }
        }

        /// <summary>
        /// Cuenta registros que cumplan la condición
        /// </summary>
        public virtual async Task<ResultadoOperacion<int>> ContarAsync(
            Expression<Func<TEntidad, bool>> filtro)
        {
            try
            {
                var cantidad = await _repositorio.ContarAsync(filtro);
                return ResultadoOperacion<int>.Ok(cantidad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al contar {typeof(TEntidad).Name}");
                return ResultadoOperacion<int>.Fallo(
                    "No pudimos contar los registros. Intenta nuevamente.");
            }
        }
    }
}
