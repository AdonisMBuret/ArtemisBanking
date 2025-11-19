using System.Linq.Expressions;

namespace ArtemisBanking.Domain.Interfaces.Repositories
{
    public interface IRepositorioGenerico<T> where T : class
    {
        Task<IEnumerable<T>> ObtenerTodosAsync();

        Task<T> ObtenerPorIdAsync(int id);

        Task<IEnumerable<T>> ObtenerPorCondicionAsync(Expression<Func<T, bool>> filtro);

        Task AgregarAsync(T entidad);

        Task ActualizarAsync(T entidad);

        Task EliminarAsync(T entidad);

         Task<int> GuardarCambiosAsync();

        Task<bool> ExisteAsync(Expression<Func<T, bool>> filtro);

        Task<int> ContarAsync(Expression<Func<T, bool>> filtro);
    }
}
