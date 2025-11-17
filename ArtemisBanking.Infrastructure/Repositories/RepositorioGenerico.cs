using ArtemisBanking.Domain.Interfaces.Repositories;
using ArtemisBanking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


namespace ArtemisBanking.Infrastructure.Repositories
{
    public class RepositorioGenerico<T> : IRepositorioGenerico<T> where T : class
    {
        protected readonly ArtemisBankingDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public RepositorioGenerico(ArtemisBankingDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        /// Obtiene todos los registros de la entidad
        public virtual async Task<IEnumerable<T>> ObtenerTodosAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// Obtiene un registro por su ID
        public virtual async Task<T> ObtenerPorIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// Obtiene registros que cumplan con una condición específica
        /// Ejemplo: repo.ObtenerPorCondicionAsync(x => x.EstaActivo == true)
        public virtual async Task<IEnumerable<T>> ObtenerPorCondicionAsync(Expression<Func<T, bool>> filtro)
        {
            return await _dbSet.Where(filtro).ToListAsync();
        }

        /// Agrega una nueva entidad a la base de datos
        public virtual async Task AgregarAsync(T entidad)
        {
            await _dbSet.AddAsync(entidad);
        }

        /// Actualiza una entidad existente
        public virtual async Task ActualizarAsync(T entidad)
        {
            _dbSet.Update(entidad);
            await Task.CompletedTask;
        }

        /// Elimina una entidad de la base de datos
        public virtual async Task EliminarAsync(T entidad)
        {
            _dbSet.Remove(entidad);
            await Task.CompletedTask;
        }

        /// Guarda todos los cambios pendientes en la base de datos
        public virtual async Task<int> GuardarCambiosAsync()
        {
            return await _context.SaveChangesAsync();
        }

        /// Verifica si existe al menos un registro que cumpla la condición
        public virtual async Task<bool> ExisteAsync(Expression<Func<T, bool>> filtro)
        {
            return await _dbSet.AnyAsync(filtro);
        }

        /// Cuenta cuántos registros cumplen la condición
        public virtual async Task<int> ContarAsync(Expression<Func<T, bool>> filtro)
        {
            return await _dbSet.CountAsync(filtro);
        }
    }
}
