using HonorCouncil_RazorPages.Data;
using HonorCouncil_RazorPages.Models;
using HonorCouncil_RazorPages.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HonorCouncil_RazorPages.Services;

public class CaseEvidenceService(
    HonorCouncilDbContext dbContext,
    ICurrentUserService currentUserService,
    IEvidenceService evidenceService) : ICaseEvidenceService
{
    public async Task<bool> CanAccessCaseAsync(int caseId, CancellationToken cancellationToken = default)
    {
        var honorCase = await dbContext.HonorCases
            .AsNoTracking()
            .Include(x => x.Report).ThenInclude(x => x.Student)
            .Include(x => x.Report).ThenInclude(x => x.FacultyMember)
            .FirstOrDefaultAsync(x => x.Id == caseId, cancellationToken);

        return honorCase is not null && CanAccessCase(honorCase);
    }

    public async Task<bool> CanAccessEvidenceAsync(int evidenceId, CancellationToken cancellationToken = default)
    {
        var honorCase = await dbContext.HonorCases
            .AsNoTracking()
            .Where(x => x.EvidenceFiles.Any(file => file.Id == evidenceId))
            .Include(x => x.Report).ThenInclude(x => x.Student)
            .Include(x => x.Report).ThenInclude(x => x.FacultyMember)
            .FirstOrDefaultAsync(cancellationToken);

        return honorCase is not null && CanAccessCase(honorCase);
    }

    public async Task<IReadOnlyList<int>> AddEvidenceAsync(int caseId, IReadOnlyList<IFormFile> files, CancellationToken cancellationToken = default)
    {
        var honorCase = await dbContext.HonorCases
            .Include(x => x.Report).ThenInclude(x => x.Student)
            .Include(x => x.Report).ThenInclude(x => x.FacultyMember)
            .FirstOrDefaultAsync(x => x.Id == caseId, cancellationToken);

        if (honorCase is null)
        {
            throw new InvalidOperationException("The requested case could not be found.");
        }

        if (!CanAccessCase(honorCase))
        {
            throw new UnauthorizedAccessException("You do not have access to this case.");
        }

        var uploads = files.Where(x => x.Length > 0).ToList();
        if (uploads.Count == 0)
        {
            throw new InvalidOperationException("Select at least one file to upload.");
        }

        var caseFolder = Path.Combine(evidenceService.GetUploadRoot(), honorCase.CaseNumber);
        Directory.CreateDirectory(caseFolder);

        var createdIds = new List<int>();
        foreach (var file in uploads)
        {
            var extension = Path.GetExtension(file.FileName);
            var storedFileName = $"{Guid.NewGuid():N}{extension}";
            var relativePath = Path.Combine(honorCase.CaseNumber, storedFileName);
            var fullPath = Path.Combine(caseFolder, storedFileName);

            await using var stream = File.Create(fullPath);
            await file.CopyToAsync(stream, cancellationToken);

            var evidence = new EvidenceFile
            {
                HonorCaseId = honorCase.Id,
                OriginalFileName = Path.GetFileName(file.FileName),
                StoredFileName = relativePath,
                ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
                FileSizeBytes = file.Length,
                UploadedByDisplayName = currentUserService.DisplayName,
                UploadedByRole = currentUserService.Role
            };

            dbContext.EvidenceFiles.Add(evidence);
            await dbContext.SaveChangesAsync(cancellationToken);
            createdIds.Add(evidence.Id);
        }

        return createdIds;
    }

    private bool CanAccessCase(HonorCase honorCase)
    {
        if (!currentUserService.IsAuthenticated)
        {
            return false;
        }

        if (currentUserService.IsInRole("Admin", "President", "Coordinator", "Investigator"))
        {
            return true;
        }

        var email = currentUserService.Email ?? string.Empty;
        if (currentUserService.IsInRole("Student"))
        {
            return string.Equals(honorCase.Report.Student.Email, email, StringComparison.OrdinalIgnoreCase);
        }

        if (currentUserService.IsInRole("Faculty"))
        {
            return string.Equals(honorCase.Report.FacultyMember?.Email, email, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }
}
