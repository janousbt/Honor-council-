using System.ComponentModel.DataAnnotations;
using HonorCouncil_RazorPages.Services.Interfaces;
using HonorCouncil_RazorPages.Services.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HonorCouncil_RazorPages.Pages.Student.Cases;

public class IndexModel(IStudentCaseService studentCaseService, ICurrentUserService currentUserService) : PageModel
{
    [BindProperty]
    public WitnessInputModel WitnessInput { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public IReadOnlyList<StudentCaseViewModel> Cases { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Cases = await studentCaseService.GetStudentCasesAsync(currentUserService.Email ?? string.Empty, cancellationToken);
        SeedWitnessCaseSelection();
    }

    public async Task<IActionResult> OnPostAddWitnessAsync(CancellationToken cancellationToken)
    {
        Cases = await studentCaseService.GetStudentCasesAsync(currentUserService.Email ?? string.Empty, cancellationToken);
        SeedWitnessCaseSelection();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await studentCaseService.AddWitnessAsync(new StudentWitnessSubmissionInput
            {
                CaseId = WitnessInput.CaseId,
                StudentEmail = currentUserService.Email ?? string.Empty,
                FullName = WitnessInput.FullName,
                Email = WitnessInput.Email,
                Affiliation = WitnessInput.Affiliation
            }, cancellationToken);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }

        StatusMessage = "Witness added successfully.";
        return RedirectToPage();
    }

    private void SeedWitnessCaseSelection()
    {
        if (WitnessInput.CaseId == 0)
        {
            WitnessInput.CaseId = Cases.FirstOrDefault()?.CaseId ?? 0;
        }
    }

    public class WitnessInputModel
    {
        [Display(Name = "Case")]
        public int CaseId { get; set; }

        [Required, Display(Name = "Name")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress, Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Affiliation")]
        public string? Affiliation { get; set; }
    }
}
