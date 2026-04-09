using System.ComponentModel.DataAnnotations;
using HonorCouncil_RazorPages.Models.Enums;
using HonorCouncil_RazorPages.Services.Interfaces;
using HonorCouncil_RazorPages.Services.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HonorCouncil_RazorPages.Pages.Admin.Appeals;

public class DetailsModel(IAppealService appealService, ICurrentUserService currentUserService) : PageModel
{
    public AppealDetailViewModel Appeal { get; private set; } = new();
    public List<SelectListItem> StatusOptions { get; private set; } = [];

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int caseId, CancellationToken cancellationToken)
    {
        return await LoadAsync(caseId, cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        await appealService.ReviewAppealAsync(new AdminAppealReviewInput
        {
            CaseId = Input.CaseId,
            Status = Input.Status,
            ReviewNotes = Input.ReviewNotes
        }, currentUserService.DisplayName, cancellationToken);

        return RedirectToPage(new { caseId = Input.CaseId });
    }

    private async Task<IActionResult> LoadAsync(int caseId, CancellationToken cancellationToken)
    {
        var appeal = await appealService.GetAppealDetailAsync(caseId, cancellationToken);
        if (appeal is null)
        {
            return NotFound();
        }

        Appeal = appeal;
        Input.CaseId = caseId;
        Input.Status = appeal.Status;
        Input.ReviewNotes = appeal.ReviewNotes;
        StatusOptions = appealService.GetAppealStatuses().Select(x => new SelectListItem(x.ToString(), x.ToString())).ToList();
        return Page();
    }

    public class InputModel
    {
        public int CaseId { get; set; }

        [Display(Name = "Appeal status")]
        public AppealStatus Status { get; set; }

        [Display(Name = "Review notes")]
        public string? ReviewNotes { get; set; }
    }
}
