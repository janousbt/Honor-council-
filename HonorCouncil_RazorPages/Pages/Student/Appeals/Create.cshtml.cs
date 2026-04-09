using System.ComponentModel.DataAnnotations;
using HonorCouncil_RazorPages.Services.Interfaces;
using HonorCouncil_RazorPages.Services.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HonorCouncil_RazorPages.Pages.Student.Appeals;

public class CreateModel(IAppealService appealService, ICurrentUserService currentUserService) : PageModel
{
    public StudentAppealPageViewModel AppealPage { get; private set; } = new();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int caseId, CancellationToken cancellationToken)
    {
        return await LoadAsync(caseId, cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return await LoadAsync(Input.CaseId, cancellationToken);
        }

        try
        {
            await appealService.SubmitAppealAsync(new StudentAppealSubmissionInput
            {
                CaseId = Input.CaseId,
                StudentEmail = currentUserService.Email ?? string.Empty,
                Grounds = Input.Grounds,
                Description = Input.Description
            }, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return await LoadAsync(Input.CaseId, cancellationToken);
        }

        return RedirectToPage("/Student/Cases/Index");
    }

    private async Task<IActionResult> LoadAsync(int caseId, CancellationToken cancellationToken)
    {
        var page = await appealService.GetStudentAppealPageAsync(caseId, currentUserService.Email ?? string.Empty, cancellationToken);
        if (page is null)
        {
            return NotFound();
        }

        AppealPage = page;
        Input.CaseId = caseId;
        return Page();
    }

    public class InputModel
    {
        public int CaseId { get; set; }

        [Required]
        public string Grounds { get; set; } = string.Empty;

        [Required, StringLength(4000)]
        public string Description { get; set; } = string.Empty;
    }
}
