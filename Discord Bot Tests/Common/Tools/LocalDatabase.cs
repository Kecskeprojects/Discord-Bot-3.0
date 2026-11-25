using DatabaseORM.Context;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Discord_Bot.Tests.Common.Tools;

//A database of its own on LocalDB's MSSQLLocalDB instance: created from the scaffolded model (with its collation and defaults),
//given the ChannelType rows from MainDB's post-deployment script, and dropped at the end. A test class gets one as a class
//fixture, so the classes still run in parallel, and empties it with ResetAsync before each of its tests.
public sealed class LocalDatabase : IAsyncLifetime
{
    public string ConnectionString { get; } =
        $"Server=(localdb)\\MSSQLLocalDB;Database=DiscordBotTests_{Guid.NewGuid():N};Integrated Security=true;TrustServerCertificate=true";

    public MainDbContext NewContext()
    {
        return new MainDbContext(new DbContextOptionsBuilder<MainDbContext>().UseSqlServer(ConnectionString).Options);
    }

    public async ValueTask InitializeAsync()
    {
        CancellationToken token = TestContext.Current.CancellationToken;
        await using MainDbContext context = NewContext();
        _ = await context.Database.EnsureCreatedAsync(token);

        //The script starts with USE [$(DatabaseName)], a SQLCMD variable that only SSDT's publish fills in
        string[] script = await File.ReadAllLinesAsync(Path.Combine(AppContext.BaseDirectory, "Database", "ChannelType_Inserts.sql"), token);
        _ = await context.Database.ExecuteSqlRawAsync(string.Join(Environment.NewLine, script.Where(line => !line.StartsWith("USE ", StringComparison.Ordinal))), token);
    }

    //Saves the rows the way earlier commands would have left them
    public async Task AddAsync(params object[] rows)
    {
        await using MainDbContext context = NewContext();
        context.AddRange(rows);
        _ = await context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    //Empties every table but the ChannelType lookup rows, with the foreign keys switched off meanwhile so the order doesn't matter
    public async Task ResetAsync()
    {
        await using MainDbContext context = NewContext();
        List<string> tables = context.Model.GetEntityTypes()
            .Where(type => type.GetTableName() is string name && name != "ChannelType")
            .Select(type => $"[{type.GetSchema() ?? "dbo"}].[{type.GetTableName()}]")
            .Distinct()
            .ToList();

        IEnumerable<string> statements = tables.Select(table => $"ALTER TABLE {table} NOCHECK CONSTRAINT ALL;")
            .Concat(tables.Select(table => $"DELETE FROM {table};"))
            .Concat(tables.Select(table => $"ALTER TABLE {table} WITH CHECK CHECK CONSTRAINT ALL;"));
        _ = await context.Database.ExecuteSqlRawAsync(string.Join(Environment.NewLine, statements), TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await using MainDbContext context = NewContext();
        _ = await context.Database.EnsureDeletedAsync();
    }
}
