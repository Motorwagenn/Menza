using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.EntityFrameworkCore;
using UTB.Minute.Db;
using Xunit;

namespace UTB.Minute.WebApi.Tests;

public class TestFixture : IAsyncLifetime
{
    private DistributedApplication app = null!;
    private string connectionString = string.Empty;

    public HttpClient HttpClient { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.UTB_Minute_AppHost>(
                ["--environment=Testing"],
                CancellationToken.None);

        app = await builder.BuildAsync(CancellationToken.None);
        await app.StartAsync(CancellationToken.None);

        await app.ResourceNotifications.WaitForResourceHealthyAsync("database");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("utb-minute-webapi");

        connectionString = await app.GetConnectionStringAsync("database")
    ?? throw new InvalidOperationException("Connection string 'database' was not found.");
        HttpClient = app.CreateHttpClient("utb-minute-webapi", "https");

        using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }

    public MinuteDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MinuteDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new MinuteDbContext(options);
    }

    public async Task ResetDatabaseAsync()
    {
        using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        HttpClient.Dispose();

        if (app is not null)
        {
            await app.DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }
    public async Task ResetDatabaseAsync(CancellationToken cancellationToken = default)
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync(cancellationToken);
        await context.Database.EnsureCreatedAsync(cancellationToken);
    }
}