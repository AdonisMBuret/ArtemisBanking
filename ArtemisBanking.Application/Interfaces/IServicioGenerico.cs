using ArtemisBanking.Application.DTOs;
using System.Linq.Expressions;

namespace ArtemisBanking.Application.Interfaces
{

    public interface IServicioGenerico<TDto, TEntidad> 
        where TDto : class 
        where TEntidad : class
    {

        Task<ResultadoOperacion<IEnumerable<TDto>>> ObtenerTodosAsync();

        Task<ResultadoOperacion<TDto>> ObtenerPorIdAsync(int id);

        Task<ResultadoOperacion<IEnumerable<TDto>>> ObtenerPorCondicionAsync(Expression<Func<TEntidad, bool>> filtro);

        Task<ResultadoOperacion<bool>> ExisteAsync(Expression<Func<TEntidad, bool>> filtro);

        Task<ResultadoOperacion<int>> ContarAsync(Expression<Func<TEntidad, bool>> filtro);
    }
}
