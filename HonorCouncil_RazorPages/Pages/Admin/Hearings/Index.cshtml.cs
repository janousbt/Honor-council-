using HonorCouncil_RazorPages.Services.Interfaces;
using HonorCouncil_RazorPages.Services.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HonorCouncil_RazorPages.Pages.Admin.Hearings;

public class IndexModel(IHearingService hearingService) : PageModel
{
    public HearingQueueViewModel Queue { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Queue = await hearingService.GetHearingQueueAsync(cancellationToken);
    }
}
