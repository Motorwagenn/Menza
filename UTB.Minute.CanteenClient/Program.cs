using Duende.AccessTokenManagement.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using UTB.Minute.CanteenClient;
using UTB.Minute.CanteenClient.Components;


var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHttpClient<CanteenService>(c => c.BaseAddress = new Uri("https://utb-minute-webapi"));

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

//keycloak componenta

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddKeycloakOpenIdConnect(
  serviceName: "keycloak",
  realm: "utb-minute",
  options =>
  {
      options.ClientId = "utb-minute-web";
      options.ClientSecret = "8r9DEYRZaEnPUmZZFNfIyWAKa0LUzWon"; // jen dev
      options.ResponseType = OpenIdConnectResponseType.Code;
      options.Scope.Add("openid");
      options.Scope.Add("offline_access");
      options.SaveTokens = true;
      options.RequireHttpsMetadata = false; // jen dev
      options.TokenValidationParameters.NameClaimType = "preferred_username";
  });

builder.Services.AddAuthorization();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddOpenIdConnectAccessTokenManagement(options =>
{
    options.RefreshBeforeExpiration = TimeSpan.FromSeconds(30);
});

builder.Services.AddUserAccessTokenHttpClient<CanteenService>(
  configureClient: (_, c) => c.BaseAddress = new Uri("https://webapi"));


var app = builder.Build();

// middleware pro autentizaci a autorizaci
app.UseAuthentication();
app.UseAuthorization();

// Pro ochranu proti CSRF u POST endpointů, které mění stav (např. logout)
app.UseAntiforgery();

app.MapDefaultEndpoints();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/login", async (HttpContext ctx, string? returnUrl) =>
{
    string redirectUri = "/";

    if (!string.IsNullOrWhiteSpace(returnUrl) && Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
    {
        redirectUri = returnUrl;
    }

    await ctx.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties
    {
        RedirectUri = redirectUri,
        IsPersistent = false
    });
});

// Logout dělám přes form a post kvůli dvojitému načítání stránky
app.MapPost("/logout", async (HttpContext ctx) =>
{
    string? idToken = await ctx.GetTokenAsync("id_token");

    await ctx.RevokeRefreshTokenAsync();

    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await ctx.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties
    {
        RedirectUri = "/students",
        Parameters = { { "id_token_hint", idToken ?? string.Empty } }
    });
});

app.Run();