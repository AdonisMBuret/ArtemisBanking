using ArtemisBanking.Application.DTOs;
using System.Linq.Expressions;

namespace ArtemisBanking.Application.Interfaces
{
    /// <summary>
    /// Interfaz genérica para servicios de aplicación
    /// Define operaciones CRUD comunes para todas las entidades
    /// </summary>
    /// <typeparam name="TDto">Tipo de DTO a retornar</typeparam>
    /// <typeparam name="TEntidad">Tipo de entidad del dominio</typeparam>
    public interface IServicioGenerico<TDto, TEntidad> 
        where TDto : class 
        where TEntidad : class
    {
        /// <summary>
        /// Obtiene todos los registros como DTOs
        /// </summary>
        Task<ResultadoOperacion<IEnumerable<TDto>>> ObtenerTodosAsync();

        /// <summary>
        /// Obtiene un registro por su ID
        /// </summary>
        Task<ResultadoOperacion<TDto>> ObtenerPorIdAsync(int id);

        /// <summary>
        /// Obtiene registros que cumplan una condición específica
        /// </summary>
        Task<ResultadoOperacion<IEnumerable<TDto>>> ObtenerPorCondicionAsync(
            Expression<Func<TEntidad, bool>> filtro);

        /// <summary>
        /// Verifica si existe un registro que cumpla la condición
        /// </summary>
        Task<ResultadoOperacion<bool>> ExisteAsync(Expression<Func<TEntidad, bool>> filtro);

        /// <summary>
        /// Cuenta registros que cumplan la condición
        /// </summary>
        Task<ResultadoOperacion<int>> ContarAsync(Expression<Func<TEntidad, bool>> filtro);
    }
}
