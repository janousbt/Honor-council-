using HonorCouncil_RazorPages.Services.Interfaces;
using HonorCouncil_RazorPages.Services.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HonorCouncil_RazorPages.Pages.Student.Schedule;

[Authorize(Policy = "StudentOnly")]
public class IndexModel(
    ICurrentUserService currentUserService,
    IStudentScheduleService studentScheduleService) : PageModel
{
    [TempData]
    public string? StatusMessage { get; set; }

    public IReadOnlyList<StudentScheduleItemViewModel> ScheduleFiles { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostUploadAsync(CancellationToken cancellationToken)
    {
        var files = (await Request.ReadFormAsync(cancellationToken)).Files;
        var file = files.FirstOrDefault();

        if (file is null || file.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Select a class schedule file to upload.");
            await LoadAsync(cancellationToken);
            return Page();
        }

        try
        {
            await studentScheduleService.UploadScheduleAsync(
                currentUserService.Email ?? string.Empty,
                currentUserService.DisplayName,
                file,
                cancellationToken);

            StatusMessage = "Class schedule uploaded successfully.";
            return RedirectToPage();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadAsync(cancellationToken);
            return Page();
        }
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        ScheduleFiles = await studentScheduleService.GetStudentSchedulesAsync(currentUserService.Email ?? string.Empty, cancellationToken);
    }
}
