using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace UTB.Minute.AdminClient;

public class UserAccessTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public UserAccessTokenHandler(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext is not null)
        {
            var accessToken = await httpContext.GetTokenAsync("access_token");

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}