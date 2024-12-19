namespace SearchAiDirectory.Utilities;

public interface IUserAuthenticator
{
    Task Authenticate(User user, bool persist);
    long GetUserID();
    string GetUserEmail();
    string GetUserName();
    string GetUserTimezone();
    long? GetProviderID();
}

public class UserAuthenticator(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : IUserAuthenticator
{
    private readonly HttpContext httpContext = httpContextAccessor.HttpContext;
    private readonly string baseURL = configuration["WebsiteUrl"].Normalize().ToString();

    public async Task Authenticate(User user, bool persist)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.ID.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Thumbprint, user.Avatar ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Locality, user.TimeZone ?? string.Empty)
        };

        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        var authProperties = new AuthenticationProperties() { RedirectUri = baseURL, IsPersistent = persist, AllowRefresh = true };

        if (httpContext.User.Identity?.IsAuthenticated ?? false) await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, authenticatedUser, authProperties);
        await Task.FromResult(new AuthenticationState(authenticatedUser));
    }

    public long GetUserID()
    {
        var userIDString = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value.Trim().Normalize();
        return Convert.ToInt64(userIDString);
    }

    public string GetUserEmail()
        => httpContext.User.Claims.SingleOrDefault(f => f.Type == ClaimTypes.Email).Value.Trim().Normalize();

    public string GetUserName()
        => httpContext.User.Claims.SingleOrDefault(f => f.Type == ClaimTypes.Name).Value.Trim().Normalize();

    public string GetUserTimezone()
        => httpContext.User.Claims.SingleOrDefault(f => f.Type == ClaimTypes.Locality).Value.Trim().Normalize();

    public long? GetProviderID()
    {
        var providerIDString = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GroupSid).Value.Trim().Normalize();
        return !string.IsNullOrEmpty(providerIDString) ? Convert.ToInt64(providerIDString) : null;
    }
}
