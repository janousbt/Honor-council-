namespace HonorCouncil_RazorPages.Services.Interfaces;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    string? Email { get; }
    string DisplayName { get; }
    string Role { get; }
    bool IsInRole(params string[] roles);
}
