using HonorCouncil_RazorPages.Data;
using HonorCouncil_RazorPages.Services;
using HonorCouncil_RazorPages.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HonorCouncil_RazorPages.Pages.Student.Evidence;

[Authorize(Policy = "StudentOnly")]
public class IndexModel(
    HonorCouncilDbContext dbContext,
    ICurrentUserService currentUserService,
    ICaseEvidenceService caseEvidenceService) : PageModel
{
    [TempData]
    public string? StatusMessage { get; set; }

    public IReadOnlyList<StudentEvidenceCaseViewModel> Cases { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadCasesAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostUploadAsync(int caseId, CancellationToken cancellationToken)
    {
        var uploadFiles = (await Request.ReadFormAsync(cancellationToken)).Files.ToList();

        if (uploadFiles.Count == 0 || uploadFiles.All(x => x.Length == 0))
        {
            ModelState.AddModelError(string.Empty, "Select at least one evidence file to upload.");
            await LoadCasesAsync(cancellationToken);
            return Page();
        }

        try
        {
            var uploadedIds = await caseEvidenceService.AddEvidenceAsync(caseId, uploadFiles, cancellationToken);
            StatusMessage = uploadedIds.Count == 1
                ? "Evidence file uploaded successfully."
                : $"{uploadedIds.Count} evidence files uploaded successfully.";

            return RedirectToPage();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadCasesAsync(cancellationToken);
            return Page();
        }
    }

    private async Task LoadCasesAsync(CancellationToken cancellationToken)
    {
        Cases = await dbContext.HonorCases
            .AsNoTracking()
            .Include(x => x.Report).ThenInclude(x => x.Course)
            .Include(x => x.Report).ThenInclude(x => x.Student)
            .Include(x => x.EvidenceFiles)
            .Where(x => x.Report.Student.Email == (currentUserService.Email ?? string.Empty))
            .OrderByDescending(x => x.Report.SubmittedUtc)
            .Select(x => new StudentEvidenceCaseViewModel
            {
                CaseId = x.Id,
                CaseNumber = x.CaseNumber,
                CourseDisplay = $"{x.Report.Course.CourseCode} - {x.Report.Course.CourseName}",
                StatusDisplay = x.CurrentStatus.ToDisplayString(),
                EvidenceFiles = x.EvidenceFiles
                    .OrderByDescending(file => file.UploadedUtc)
                    .Select(file => new StudentEvidenceFileViewModel
                    {
                        Id = file.Id,
                        FileName = file.OriginalFileName,
                        UploadedByDisplayName = string.IsNullOrWhiteSpace(file.UploadedByDisplayName)
                            ? file.UploadedByRole
                            : file.UploadedByDisplayName,
                        UploadedUtcDisplay = file.UploadedUtc.ToLocalTime().ToString("MMMM d, yyyy h:mm tt")
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);
    }

    public class StudentEvidenceCaseViewModel
    {
        public int CaseId { get; set; }
        public string CaseNumber { get; set; } = string.Empty;
        public string CourseDisplay { get; set; } = string.Empty;
        public string StatusDisplay { get; set; } = string.Empty;
        public IReadOnlyList<StudentEvidenceFileViewModel> EvidenceFiles { get; set; } = [];
    }

    public class StudentEvidenceFileViewModel
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string UploadedByDisplayName { get; set; } = string.Empty;
        public string UploadedUtcDisplay { get; set; } = string.Empty;
    }
}
