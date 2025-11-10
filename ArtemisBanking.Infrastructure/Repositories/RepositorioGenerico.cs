using ArtemisBanking.Domain.Interfaces;
using ArtemisBanking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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

        /// <summary>
        /// Obtiene todos los registros de la entidad
        /// </summary>
        public virtual async Task<IEnumerable<T>> ObtenerTodosAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// <summary>
        /// Obtiene un registro por su ID
        /// </summary>
        public virtual async Task<T> ObtenerPorIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <summary>
        /// Obtiene registros que cumplan con una condición específica
        /// Ejemplo: repo.ObtenerPorCondicionAsync(x => x.EstaActivo == true)
        /// </summary>
        public virtual async Task<IEnumerable<T>> ObtenerPorCondicionAsync(Expression<Func<T, bool>> filtro)
        {
            return await _dbSet.Where(filtro).ToListAsync();
        }

        /// <summary>
        /// Agrega una nueva entidad a la base de datos
        /// </summary>
        public virtual async Task AgregarAsync(T entidad)
        {
            await _dbSet.AddAsync(entidad);
        }

        /// <summary>
        /// Actualiza una entidad existente
        /// </summary>
        public virtual async Task ActualizarAsync(T entidad)
        {
            _dbSet.Update(entidad);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Elimina una entidad de la base de datos
        /// </summary>
        public virtual async Task EliminarAsync(T entidad)
        {
            _dbSet.Remove(entidad);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Guarda todos los cambios pendientes en la base de datos
        /// </summary>
        public virtual async Task<int> GuardarCambiosAsync()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Verifica si existe al menos un registro que cumpla la condición
        /// </summary>
        public virtual async Task<bool> ExisteAsync(Expression<Func<T, bool>> filtro)
        {
            return await _dbSet.AnyAsync(filtro);
        }

        /// <summary>
        /// Cuenta cuántos registros cumplen la condición
        /// </summary>
        public virtual async Task<int> ContarAsync(Expression<Func<T, bool>> filtro)
        {
            return await _dbSet.CountAsync(filtro);
        }
    }
}
