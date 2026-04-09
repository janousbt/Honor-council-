using HonorCouncil_RazorPages.Services.Interfaces;
using HonorCouncil_RazorPages.Services.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HonorCouncil_RazorPages.Pages.Admin.Appeals;

public class IndexModel(IAppealService appealService) : PageModel
{
    public IReadOnlyList<AppealQueueItemViewModel> Appeals { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Appeals = await appealService.GetAppealQueueAsync(cancellationToken);
    }
}
