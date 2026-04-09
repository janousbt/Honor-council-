using HonorCouncil_RazorPages.Services.Interfaces;
using HonorCouncil_RazorPages.Services.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HonorCouncil_RazorPages.Pages.Admin;

public class DashboardModel(ICurrentUserService currentUserService, IAdminCaseService adminCaseService) : PageModel
{
    public string CurrentRole { get; private set; } = string.Empty;
    public DashboardSummaryViewModel Summary { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        CurrentRole = currentUserService.Role;
        Summary = await adminCaseService.GetDashboardSummaryAsync(cancellationToken);
    }
}
