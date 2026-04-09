using HonorCouncil_RazorPages.Models.Enums;
using HonorCouncil_RazorPages.Services.Interfaces;
using HonorCouncil_RazorPages.Services.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HonorCouncil_RazorPages.Pages.Admin.Cases;

public class IndexModel(IAdminCaseService adminCaseService) : PageModel
{
    public CaseQueueViewModel Queue { get; private set; } = new();
    public IReadOnlyList<string> StatusOptions { get; } = Enum.GetNames<CaseStatus>();

    public async Task OnGetAsync([FromQuery] string? statusFilter, [FromQuery] string? sort, CancellationToken cancellationToken)
    {
        Queue = await adminCaseService.GetCaseQueueAsync(statusFilter, sort, cancellationToken);
    }
}
