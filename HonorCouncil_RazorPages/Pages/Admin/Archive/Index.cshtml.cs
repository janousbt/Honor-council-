using HonorCouncil_RazorPages.Services.Interfaces;
using HonorCouncil_RazorPages.Services.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HonorCouncil_RazorPages.Pages.Admin.Archive;

public class IndexModel(IAdminCaseService adminCaseService) : PageModel
{
    public ArchivedCaseListViewModel Archive { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Archive = await adminCaseService.GetArchivedCasesAsync(cancellationToken);
    }
}
