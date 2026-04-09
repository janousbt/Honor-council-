using HonorCouncil_RazorPages.Services.Interfaces;
using HonorCouncil_RazorPages.Services.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HonorCouncil_RazorPages.Pages.Faculty;

public class DashboardModel(ICurrentUserService currentUserService, IFacultyCaseService facultyCaseService) : PageModel
{
    public FacultyDashboardSummaryViewModel Summary { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Summary = await facultyCaseService.GetDashboardSummaryAsync(currentUserService.Email ?? string.Empty, cancellationToken);
    }
}
