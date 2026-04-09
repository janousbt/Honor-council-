using HonorCouncil_RazorPages.Models;

namespace HonorCouncil_RazorPages.Services.Interfaces;

public interface IApplicationAuthenticationService
{
    Task<ApplicationUser?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default);
}
