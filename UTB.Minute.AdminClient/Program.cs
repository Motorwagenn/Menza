using UTB.Minute.AdminClient.Components;
using UTB.Minute.AdminClient;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<UserAccessTokenHandler>();

builder.Services.AddHttpClient<AdminService>(
    c => c.BaseAddress = new Uri("https://utb-minute-webapi"))
    .AddHttpMessageHandler<UserAccessTokenHandler>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(options =>
{
    options.Authority = "https://localhost:8080/realms/utb-minute";
    options.MetadataAddress = "https://localhost:8080/realms/utb-minute/.well-known/openid-configuration";

    options.ClientId = "utb-minute-admin";
    options.ClientSecret = "cQMAK3JIeYZ40JmIMshYWAVXYeFIdriH";

    options.ResponseType = OpenIdConnectResponseType.Code;
    options.SaveTokens = true;
    options.RequireHttpsMetadata = false;
    options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Disable;

    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.Scope.Add("offline_access");

    options.TokenValidationParameters.NameClaimType = "preferred_username";
});

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapGet("/login", async (HttpContext ctx, string? returnUrl) =>
{
    string redirectUri = "/";

    if (!string.IsNullOrWhiteSpace(returnUrl) &&
        Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
    {
        redirectUri = returnUrl;
    }

    await ctx.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme,
        new AuthenticationProperties
        {
            RedirectUri = redirectUri,
            IsPersistent = false
        });
});

app.MapPost("/logout", async (HttpContext ctx) =>
{
    string? idToken = await ctx.GetTokenAsync("id_token");

    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await ctx.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme,
        new AuthenticationProperties
        {
            RedirectUri = "/",
            Parameters = { { "id_token_hint", idToken ?? string.Empty } }
        });
});

app.Run();