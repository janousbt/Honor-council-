using System.Security.Claims;
using HonorCouncil_RazorPages.Services.Interfaces;

namespace HonorCouncil_RazorPages.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
    public string? Email => User?.FindFirstValue(ClaimTypes.Email);
    public string DisplayName => User?.FindFirstValue(ClaimTypes.Name) ?? "Guest";
    public string Role => User?.FindFirstValue(ClaimTypes.Role) ?? "Anonymous";

    public bool IsInRole(params string[] roles) => roles.Any(role => string.Equals(Role, role, StringComparison.OrdinalIgnoreCase));
}
