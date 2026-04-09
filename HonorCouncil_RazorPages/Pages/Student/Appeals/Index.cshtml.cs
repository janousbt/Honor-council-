using HonorCouncil_RazorPages.Services.Interfaces;
using HonorCouncil_RazorPages.Services.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HonorCouncil_RazorPages.Pages.Student.Appeals;

public class IndexModel(IAppealService appealService, ICurrentUserService currentUserService) : PageModel
{
    public IReadOnlyList<StudentAppealCaseViewModel> AppealCases { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        AppealCases = await appealService.GetStudentAppealCasesAsync(currentUserService.Email ?? string.Empty, cancellationToken);
    }
}
