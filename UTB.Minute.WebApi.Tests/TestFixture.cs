using System.Net.Http.Headers;
using System.Net.Http.Json;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.EntityFrameworkCore;
using UTB.Minute.Db;
using Xunit;

namespace UTB.Minute.WebApi.Tests;

public class TestFixture : IAsyncLifetime
{
    private DistributedApplication app = null!;
    private HttpClient? keycloakClient;
    private string? idToken;
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

        // čekání na resource
        await app.ResourceNotifications.WaitForResourceHealthyAsync("keycloak");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("database");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("utb-minute-webapi");

        // vytvoření clienta pro Keycloak
        keycloakClient = app.CreateHttpClient("keycloak", "https");

        // login do Keycloaku
        var response = await keycloakClient.PostAsync(
            "/realms/utb-minute/protocol/openid-connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "client_id", "utb-minute-tests" },
                { "username", "Michael" },
                { "password", "michael" },
                { "scope", "openid" }
            }));

        response.EnsureSuccessStatusCode();

        // parsování token response
        TokenResponse tokenResponse =
            await response.Content.ReadFromJsonAsync<TokenResponse>()
            ?? throw new Xunit.Sdk.XunitException(
                "Token endpoint returned null TokenResponse.");

        if (string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
        {
            throw new Xunit.Sdk.XunitException(
                "TokenResponse does not contain AccessToken.");
        }

        idToken = tokenResponse.IdToken;

        // WebApi client
        HttpClient = app.CreateHttpClient("utb-minute-webapi", "https");

        // Bearer token do hlavičky
        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                tokenResponse.AccessToken);

        connectionString = await app.GetConnectionStringAsync("database")
            ?? throw new InvalidOperationException(
                "Connection string 'database' was not found.");

        // reset DB
        await using var context = CreateContext();

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

    public async Task ResetDatabaseAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = CreateContext();

        await context.Database.EnsureDeletedAsync(cancellationToken);
        await context.Database.EnsureCreatedAsync(cancellationToken);
    }

    public async Task DisposeAsync()
    {
        // logout z Keycloaku
        if (keycloakClient is not null)
        {
            if (!string.IsNullOrWhiteSpace(idToken))
            {
                _ = await keycloakClient.PostAsync(
                    "/realms/utb-minute/protocol/openid-connect/logout",
                    new FormUrlEncodedContent(
                        new Dictionary<string, string>
                        {
                            { "id_token_hint", idToken }
                        }));
            }

            keycloakClient.Dispose();
        }

        HttpClient?.Dispose();

        if (app is not null)
        {
            await app.DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }
}