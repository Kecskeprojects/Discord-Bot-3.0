using DatabaseORM.Interface.Repository;
using NSubstitute.Core;
using NSubstitute.ExceptionExtensions;
using System.Linq.Expressions;

namespace Discord_Bot.Tests.Mocks.Database;

//Makes a substitute repository work like a table of the given rows: its queries run the service's own predicate on the rows, and
//adds and removes change the list that HasRows returns. It differs from SQL Server in two ways: string comparisons are
//case-sensitive here, and every navigation a row has set counts as loaded, so a missing include goes unnoticed.
internal static class RepositoryFakes
{
    public static List<TEntity> HasRows<TEntity>(this IGenericRepository<TEntity> repository, params IEnumerable<TEntity> rows) where TEntity : class
    {
        List<TEntity> table = rows.ToList();

        repository.FirstOrDefaultAsync(AnyPredicate<TEntity>()).Returns(call => table.FirstOrDefault(Predicate<TEntity>(call)));
        repository.FirstOrDefaultAsync(AnyPredicate<TEntity>(), AnyIncludes<TEntity>()).Returns(call => table.FirstOrDefault(Predicate<TEntity>(call)));
        repository.ExistsAsync(AnyPredicate<TEntity>()).Returns(call => table.Any(Predicate<TEntity>(call)));
        repository.ExistsAsync(AnyPredicate<TEntity>(), AnyIncludes<TEntity>()).Returns(call => table.Any(Predicate<TEntity>(call)));

        repository.GetListAsync(AnyPredicate<TEntity>()).Returns(call => table.Where(Predicate<TEntity>(call)).ToList());
        repository.GetListAsync(AnyPredicate<TEntity>(), AnyIncludes<TEntity>()).Returns(call => table.Where(Predicate<TEntity>(call)).ToList());
        repository.GetListAsync(AnyPredicate<TEntity>(), AnyOrder<TEntity>(), Arg.Any<bool>())
            .Returns(call => Ordered(table.Where(Predicate<TEntity>(call)), call, 1));
        repository.GetListAsync(AnyPredicate<TEntity>(), AnyOrder<TEntity>(), Arg.Any<bool>(), AnyIncludes<TEntity>())
            .Returns(call => Ordered(table.Where(Predicate<TEntity>(call)), call, 1));

        repository.GetAllAsync().Returns(_ => table.ToList());
        repository.GetAllAsync(AnyIncludes<TEntity>()).Returns(_ => table.ToList());
        repository.GetAllAsync(AnyOrder<TEntity>(), Arg.Any<bool>()).Returns(call => Ordered(table, call, 0));
        repository.GetAllAsync(AnyOrder<TEntity>(), Arg.Any<bool>(), AnyIncludes<TEntity>()).Returns(call => Ordered(table, call, 0));

        repository.AddAsync(Arg.Any<TEntity>(), Arg.Any<bool>()).Returns(call =>
        {
            table.Add(call.ArgAt<TEntity>(0));
            return 1;
        });
        repository.AddAsync(Arg.Any<IEnumerable<TEntity>>(), Arg.Any<bool>()).Returns(call =>
        {
            List<TEntity> items = call.ArgAt<IEnumerable<TEntity>>(0).ToList();
            table.AddRange(items);
            return items.Count;
        });
        repository.RemoveAsync(Arg.Any<TEntity>(), Arg.Any<bool>()).Returns(call => table.Remove(call.ArgAt<TEntity>(0)) ? 1 : 0);
        repository.RemoveAsync(Arg.Any<IEnumerable<TEntity>>(), Arg.Any<bool>())
            .Returns(call => call.ArgAt<IEnumerable<TEntity>>(0).ToList().Count(table.Remove));
        repository.UpdateAsync(Arg.Any<TEntity>(), Arg.Any<bool>()).Returns(1);
        repository.UpdateAsync(Arg.Any<IEnumerable<TEntity>>(), Arg.Any<bool>()).Returns(call => call.ArgAt<IEnumerable<TEntity>>(0).Count());
        repository.SaveChangesAsync().Returns(1);

        return table;
    }

    //Every query and write throws, as when the database can't be reached
    public static void Fails<TEntity>(this IGenericRepository<TEntity> repository) where TEntity : class
    {
        InvalidOperationException error = new("The database can't be reached.");

        repository.FirstOrDefaultAsync(AnyPredicate<TEntity>()).ThrowsAsync(error);
        repository.FirstOrDefaultAsync(AnyPredicate<TEntity>(), AnyIncludes<TEntity>()).ThrowsAsync(error);
        repository.ExistsAsync(AnyPredicate<TEntity>()).ThrowsAsync(error);
        repository.ExistsAsync(AnyPredicate<TEntity>(), AnyIncludes<TEntity>()).ThrowsAsync(error);

        repository.GetListAsync(AnyPredicate<TEntity>()).ThrowsAsync(error);
        repository.GetListAsync(AnyPredicate<TEntity>(), AnyIncludes<TEntity>()).ThrowsAsync(error);
        repository.GetListAsync(AnyPredicate<TEntity>(), AnyOrder<TEntity>(), Arg.Any<bool>()).ThrowsAsync(error);
        repository.GetListAsync(AnyPredicate<TEntity>(), AnyOrder<TEntity>(), Arg.Any<bool>(), AnyIncludes<TEntity>()).ThrowsAsync(error);

        repository.GetAllAsync().ThrowsAsync(error);
        repository.GetAllAsync(AnyIncludes<TEntity>()).ThrowsAsync(error);
        repository.GetAllAsync(AnyOrder<TEntity>(), Arg.Any<bool>()).ThrowsAsync(error);
        repository.GetAllAsync(AnyOrder<TEntity>(), Arg.Any<bool>(), AnyIncludes<TEntity>()).ThrowsAsync(error);

        repository.AddAsync(Arg.Any<TEntity>(), Arg.Any<bool>()).ThrowsAsync(error);
        repository.AddAsync(Arg.Any<IEnumerable<TEntity>>(), Arg.Any<bool>()).ThrowsAsync(error);
        repository.RemoveAsync(Arg.Any<TEntity>(), Arg.Any<bool>()).ThrowsAsync(error);
        repository.RemoveAsync(Arg.Any<IEnumerable<TEntity>>(), Arg.Any<bool>()).ThrowsAsync(error);
        repository.UpdateAsync(Arg.Any<TEntity>(), Arg.Any<bool>()).ThrowsAsync(error);
        repository.UpdateAsync(Arg.Any<IEnumerable<TEntity>>(), Arg.Any<bool>()).ThrowsAsync(error);
        repository.SaveChangesAsync().ThrowsAsync(error);
    }

    private static Expression<Func<TEntity, bool>> AnyPredicate<TEntity>()
    {
        return Arg.Any<Expression<Func<TEntity, bool>>>();
    }

    private static Expression<Func<TEntity, object?>> AnyOrder<TEntity>()
    {
        return Arg.Any<Expression<Func<TEntity, object?>>>();
    }

    private static Expression<Func<TEntity, object?>>[] AnyIncludes<TEntity>()
    {
        return Arg.Any<Expression<Func<TEntity, object?>>[]>();
    }

    private static Func<TEntity, bool> Predicate<TEntity>(CallInfo call)
    {
        return call.ArgAt<Expression<Func<TEntity, bool>>>(0).Compile();
    }

    //The order expression is at orderIndex, the ascending flag right after it
    private static List<TEntity> Ordered<TEntity>(IEnumerable<TEntity> rows, CallInfo call, int orderIndex)
    {
        Func<TEntity, object?> key = call.ArgAt<Expression<Func<TEntity, object?>>>(orderIndex).Compile();
        return (call.ArgAt<bool>(orderIndex + 1) ? rows.OrderBy(key) : rows.OrderByDescending(key)).ToList();
    }
}
