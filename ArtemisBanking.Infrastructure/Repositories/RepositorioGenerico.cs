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

        public virtual async Task<IEnumerable<T>> ObtenerTodosAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<T> ObtenerPorIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> ObtenerPorCondicionAsync(Expression<Func<T, bool>> filtro)
        {
            return await _dbSet.Where(filtro).ToListAsync();
        }

        public virtual async Task AgregarAsync(T entidad)
        {
            await _dbSet.AddAsync(entidad);
        }

        public virtual async Task ActualizarAsync(T entidad)
        {
            _dbSet.Update(entidad);
            await Task.CompletedTask;
        }

        public virtual async Task EliminarAsync(T entidad)
        {
            _dbSet.Remove(entidad);
            await Task.CompletedTask;
        }

        public virtual async Task<int> GuardarCambiosAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public virtual async Task<bool> ExisteAsync(Expression<Func<T, bool>> filtro)
        {
            return await _dbSet.AnyAsync(filtro);
        }

        public virtual async Task<int> ContarAsync(Expression<Func<T, bool>> filtro)
        {
            return await _dbSet.CountAsync(filtro);
        }
    }
}
