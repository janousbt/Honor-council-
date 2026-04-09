using HonorCouncil_RazorPages.Data;
using HonorCouncil_RazorPages.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HonorCouncil_RazorPages.Pages.Evidence;

[Authorize]
public class DownloadModel(
    HonorCouncilDbContext dbContext,
    IEvidenceService evidenceService,
    ICaseEvidenceService caseEvidenceService) : PageModel
{
    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        if (!await caseEvidenceService.CanAccessEvidenceAsync(id, cancellationToken))
        {
            return Forbid();
        }

        var evidence = await dbContext.EvidenceFiles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (evidence is null)
        {
            return NotFound();
        }

        var path = Path.Combine(evidenceService.GetUploadRoot(), evidence.StoredFileName);
        if (!System.IO.File.Exists(path))
        {
            return NotFound();
        }

        var bytes = await System.IO.File.ReadAllBytesAsync(path, cancellationToken);
        return File(bytes, evidence.ContentType, evidence.OriginalFileName);
    }
}
