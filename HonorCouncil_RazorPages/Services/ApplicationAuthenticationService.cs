using HonorCouncil_RazorPages.Data;
using HonorCouncil_RazorPages.Models;
using HonorCouncil_RazorPages.Options;
using HonorCouncil_RazorPages.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HonorCouncil_RazorPages.Services;

public class ApplicationAuthenticationService(
    HonorCouncilDbContext dbContext,
    IPasswordHasher<ApplicationUser> passwordHasher,
    IOptions<SeedUserOptions> options) : IApplicationAuthenticationService
{
    private readonly SeedUserOptions _options = options.Value;

    public async Task<ApplicationUser?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim();
        var user = await dbContext.ApplicationUsers
            .FirstOrDefaultAsync(account => account.Email == normalizedEmail, cancellationToken);

        if (user is not null)
        {
            var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return verification == PasswordVerificationResult.Failed || !user.IsActive ? null : user;
        }

        var seededUser = _options.Accounts.FirstOrDefault(account =>
            string.Equals(account.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(account.Password, password, StringComparison.Ordinal));

        if (seededUser is null)
        {
            return null;
        }

        return await dbContext.ApplicationUsers
            .FirstOrDefaultAsync(account => account.Email == seededUser.Email, cancellationToken);
    }
}
