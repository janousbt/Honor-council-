using System.ComponentModel.DataAnnotations;
using HonorCouncil_RazorPages.Models.Enums;
using HonorCouncil_RazorPages.Services.Interfaces;
using HonorCouncil_RazorPages.Services.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HonorCouncil_RazorPages.Pages.Student.Availability;

public class EditModel(IHearingService hearingService, ICurrentUserService currentUserService) : PageModel
{
    public IReadOnlyList<ParticipantCaseOptionViewModel> AvailableCases { get; private set; } = [];

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task OnGetAsync(int? caseId, CancellationToken cancellationToken)
    {
        await LoadCasesAsync(caseId, cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        AvailableCases = await hearingService.GetStudentCasesAsync(currentUserService.Email ?? string.Empty, cancellationToken);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await hearingService.SubmitAvailabilityAsync(new AvailabilitySubmissionInput
            {
                CaseId = Input.CaseId,
                SubmittedByName = currentUserService.DisplayName,
                SubmittedByEmail = currentUserService.Email ?? string.Empty,
                ParticipantRole = AvailabilityParticipantRole.Student,
                StartLocal = Input.StartLocal,
                EndLocal = Input.EndLocal
            }, cancellationToken);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }

        return RedirectToPage(new { caseId = Input.CaseId });
    }

    private async Task LoadCasesAsync(int? caseId, CancellationToken cancellationToken)
    {
        AvailableCases = await hearingService.GetStudentCasesAsync(currentUserService.Email ?? string.Empty, cancellationToken);
        var selectedCaseId = caseId ?? AvailableCases.FirstOrDefault()?.CaseId ?? 0;
        Input.CaseId = selectedCaseId;
        Input.StartLocal = DateTime.Now.AddDays(1).Date.AddHours(9);
        Input.EndLocal = DateTime.Now.AddDays(1).Date.AddHours(10);
    }

    public class InputModel
    {
        public int CaseId { get; set; }

        [Required, Display(Name = "Available from")]
        public DateTime StartLocal { get; set; }

        [Required, Display(Name = "Available until")]
        public DateTime EndLocal { get; set; }
    }
}
