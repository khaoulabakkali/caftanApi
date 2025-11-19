using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace mkBoutiqueCaftan.Services;

public interface IUserContextService
{
    int? GetIdUtilisateur();
    string? GetLogin();
}

public class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? GetIdUtilisateur()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var idUtilisateurClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? httpContext.User.FindFirst("IdUtilisateur")?.Value;
        
        if (string.IsNullOrEmpty(idUtilisateurClaim))
        {
            return null;
        }

        if (int.TryParse(idUtilisateurClaim, out var idUtilisateur))
        {
            return idUtilisateur;
        }

        return null;
    }

    public string? GetLogin()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return httpContext.User.FindFirst(ClaimTypes.Name)?.Value 
            ?? httpContext.User.FindFirst("Login")?.Value;
    }
}

