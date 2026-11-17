using System.Linq.Expressions;

namespace DatabaseORM.Tools.Extensions;

internal static class EntityFrameworkExtension
{
    public static IOrderedQueryable<TEntity> OrderByDynamic<TEntity, TKey>(this IQueryable<TEntity> entities, Expression<Func<TEntity, TKey>> keySelector, bool ascending = true)
    {
        return ascending ?
            entities.OrderBy(keySelector) :
            entities.OrderByDescending(keySelector);
    }
}
