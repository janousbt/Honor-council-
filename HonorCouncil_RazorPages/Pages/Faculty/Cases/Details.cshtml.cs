using HonorCouncil_RazorPages.Services.Interfaces;
using HonorCouncil_RazorPages.Services.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HonorCouncil_RazorPages.Pages.Faculty.Cases;

public class DetailsModel(ICurrentUserService currentUserService, IFacultyCaseService facultyCaseService) : PageModel
{
    public CaseDetailViewModel CaseDetail { get; private set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        return await LoadPageAsync(id, cancellationToken);
    }

    public async Task<IActionResult> OnPostUploadEvidenceAsync([FromServices] ICaseEvidenceService caseEvidenceService, int id, CancellationToken cancellationToken)
    {
        var uploadFiles = (await Request.ReadFormAsync(cancellationToken)).Files.ToList();

        if (uploadFiles.Count == 0 || uploadFiles.All(x => x.Length == 0))
        {
            ModelState.Clear();
            ModelState.AddModelError(string.Empty, "Select at least one evidence file to upload.");
            return await LoadPageAsync(id, cancellationToken);
        }

        try
        {
            var uploadedIds = await caseEvidenceService.AddEvidenceAsync(id, uploadFiles, cancellationToken);
            StatusMessage = uploadedIds.Count == 1
                ? "Evidence file uploaded successfully."
                : $"{uploadedIds.Count} evidence files uploaded successfully.";
            return RedirectToPage(new { id });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.Clear();
            ModelState.AddModelError(string.Empty, ex.Message);
            return await LoadPageAsync(id, cancellationToken);
        }
    }

    private async Task<IActionResult> LoadPageAsync(int id, CancellationToken cancellationToken)
    {
        var caseDetail = await facultyCaseService.GetCaseDetailAsync(id, currentUserService.Email ?? string.Empty, cancellationToken);
        if (caseDetail is null)
        {
            return NotFound();
        }

        CaseDetail = caseDetail;
        return Page();
    }
}
