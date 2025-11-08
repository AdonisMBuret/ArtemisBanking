using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisBanking.Domain.Interfaces
{
    public interface IRepositorioGenerico<T> where T : class
    {
        // Obtener todos los registros
        Task<IEnumerable<T>> ObtenerTodosAsync();

        // Obtener un registro por su Id
        Task<T> ObtenerPorIdAsync(int id);

        // Obtener registros que cumplan una condición
        Task<IEnumerable<T>> ObtenerPorCondicionAsync(Expression<Func<T, bool>> filtro);

        // Agregar un nuevo registro
        Task AgregarAsync(T entidad);

        // Actualizar un registro existente
        Task ActualizarAsync(T entidad);

        // Eliminar un registro
        Task EliminarAsync(T entidad);

        // Guardar cambios en la base de datos
        Task<int> GuardarCambiosAsync();

        // Verificar si existe un registro que cumpla una condición
        Task<bool> ExisteAsync(Expression<Func<T, bool>> filtro);

        // Contar registros que cumplan una condición
        Task<int> ContarAsync(Expression<Func<T, bool>> filtro);
    }
}
