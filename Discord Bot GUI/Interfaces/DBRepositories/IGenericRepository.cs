using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Discord_Bot.Interfaces.DBRepositories
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        ValueTask<EntityEntry<TEntity>> AddAsync(TEntity item);
        Task AddAsync(IEnumerable<TEntity> items);
        ValueTask<TEntity> FindByIdAsync(int id);
        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes);
        Task<List<TEntity>> GetAllAsync();
        Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, object>> orderBy, bool ascending = true);
        Task<List<TEntity>> GetAllAsync(params Expression<Func<TEntity, object>>[] includes);
        Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, object>> orderBy, bool ascending = true, params Expression<Func<TEntity, object>>[] includes);
        Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate);
        Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> orderBy, bool ascending = true);
        Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes);
        Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> orderBy, bool ascending = true, params Expression<Func<TEntity, object>>[] includes);
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes);
        Task<int> RemoveAsync(TEntity item, bool saveChanges = true);
        Task<int> RemoveAsync(IEnumerable<TEntity> items, bool saveChanges = true);
        Task<int> SaveChangesAsync();
        Task<int> UpdateAsync(TEntity item, bool saveChanges = true);
        Task<int> UpdateAsync(IEnumerable<TEntity> items, bool saveChanges = true);
    }
}
