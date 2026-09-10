using Application.Interfaces.Repositories.User;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;

namespace EduGate.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value
                         ?? _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value;

    public IEnumerable<string> Roles
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
                return Enumerable.Empty<string>();

            // FindAll بتجيب كل الرتب بدون ما تضيع أي رول سواء تكررت بـ ClaimTypes.Role أو بـ "role"
            return user.FindAll(ClaimTypes.Role)
                       .Concat(user.FindAll("role"))
                       .Select(c => c.Value)
                       .Distinct();
        }
    }
    public string? GetClaimValue(string claimType)
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(claimType)?.Value;
    }

    public Guid? GetClaimAsGuid(string claimType)
    {
        var value = GetClaimValue(claimType);
        return value != null && Guid.TryParse(value, out var result) ? result : null;
    }
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}