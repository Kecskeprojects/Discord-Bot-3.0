using Discord_Bot.Interfaces.DBRepositories;
using Discord_Bot.Tools;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Discord_Bot.Database.DBRepositories
{
    public class GenericRepository<TEntity>(MainDbContext context) : BaseRepository(context), IGenericRepository<TEntity> where TEntity : class
    {
        #region Add
        public async Task<int> AddAsync(TEntity item, bool saveChanges = true)
        {
            context.Set<TEntity>().Add(item);

            return saveChanges ? await context.SaveChangesAsync() : 0;
        }

        public async Task<int> AddAsync(IEnumerable<TEntity> items, bool saveChanges = true)
        {
            context.Set<TEntity>().AddRange(items);

            return saveChanges ? await context.SaveChangesAsync() : 0;
        }
        #endregion

        #region Exists
        public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return context.Set<TEntity>().Where(predicate).AnyAsync();
        }

        public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> dbSet = context.Set<TEntity>().Where(predicate);

            foreach (Expression<Func<TEntity, object>> include in includes)
            {
                dbSet = dbSet.Include(include);
            }

            return dbSet.AnyAsync();
        }
        #endregion

        #region FindById
        public ValueTask<TEntity> FindByIdAsync(int id)
        {
            return context.Set<TEntity>().FindAsync(id);
        }
        #endregion

        #region FirstOrDefault
        public Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return context.Set<TEntity>().FirstOrDefaultAsync(predicate);
        }

        public Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> dbSet = context.Set<TEntity>();

            foreach (Expression<Func<TEntity, object>> include in includes)
            {
                dbSet = dbSet.Include(include);
            }

            return dbSet.FirstOrDefaultAsync(predicate);
        }
        #endregion

        #region GetAll
        public Task<List<TEntity>> GetAllAsync()
        {
            return context.Set<TEntity>().ToListAsync();
        }

        public Task<List<TEntity>> GetAllAsync(params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> dbSet = context.Set<TEntity>();

            foreach (Expression<Func<TEntity, object>> include in includes)
            {
                dbSet = dbSet.Include(include);
            }

            return dbSet.ToListAsync();
        }

        public Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, object>> orderBy, bool ascending = true)
        {
            IQueryable<TEntity> dbSet = context.Set<TEntity>();

            dbSet = dbSet.OrderByDynamic(orderBy, ascending);

            return dbSet.ToListAsync();
        }

        public Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, object>> orderBy, bool ascending = true, params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> dbSet = context.Set<TEntity>();

            dbSet = dbSet.OrderByDynamic(orderBy, ascending);

            foreach (Expression<Func<TEntity, object>> include in includes)
            {
                dbSet = dbSet.Include(include);
            }

            return dbSet.ToListAsync();
        }
        #endregion

        #region GetList
        public Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return context.Set<TEntity>().Where(predicate).ToListAsync();
        }

        public Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> dbSet = context.Set<TEntity>().Where(predicate);

            foreach (Expression<Func<TEntity, object>> include in includes)
            {
                dbSet = dbSet.Include(include);
            }

            return dbSet.ToListAsync();
        }

        public Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> orderBy, bool ascending = true)
        {
            IQueryable<TEntity> dbSet = context.Set<TEntity>().Where(predicate);

            dbSet = dbSet.OrderByDynamic(orderBy, ascending);

            return dbSet.ToListAsync();
        }

        public Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> orderBy, bool ascending = true, params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> dbSet = context.Set<TEntity>().Where(predicate);

            dbSet = dbSet.OrderByDynamic(orderBy, ascending);

            foreach (Expression<Func<TEntity, object>> include in includes)
            {
                dbSet = dbSet.Include(include);
            }

            return dbSet.ToListAsync();
        }
        #endregion

        #region Remove
        public async Task<int> RemoveAsync(TEntity item, bool saveChanges = true)
        {
            context.Set<TEntity>().Remove(item);

            return saveChanges ? await context.SaveChangesAsync() : 0;
        }

        public async Task<int> RemoveAsync(IEnumerable<TEntity> items, bool saveChanges = true)
        {
            context.Set<TEntity>().RemoveRange(items);

            return saveChanges ? await context.SaveChangesAsync() : 0;
        }
        #endregion

        #region SaveChanges
        public Task<int> SaveChangesAsync()
        {
            return context.SaveChangesAsync();
        }
        #endregion

        #region Update
        public async Task<int> UpdateAsync(TEntity item, bool saveChanges = true)
        {
            context.Set<TEntity>().Update(item);

            return saveChanges ? await context.SaveChangesAsync() : 0;
        }

        public async Task<int> UpdateAsync(IEnumerable<TEntity> items, bool saveChanges = true)
        {
            context.Set<TEntity>().UpdateRange(items);

            return saveChanges ? await context.SaveChangesAsync() : 0;
        }
        #endregion
    }
}
