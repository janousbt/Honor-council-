using Microsoft.AspNetCore.Http;

namespace HonorCouncil_RazorPages.Services.Interfaces;

public interface ICaseEvidenceService
{
    Task<bool> CanAccessCaseAsync(int caseId, CancellationToken cancellationToken = default);
    Task<bool> CanAccessEvidenceAsync(int evidenceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> AddEvidenceAsync(int caseId, IReadOnlyList<IFormFile> files, CancellationToken cancellationToken = default);
}
