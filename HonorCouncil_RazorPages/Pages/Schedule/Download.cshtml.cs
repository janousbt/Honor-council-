using HonorCouncil_RazorPages.Data;
using HonorCouncil_RazorPages.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HonorCouncil_RazorPages.Pages.Schedule;

[Authorize]
public class DownloadModel(
    HonorCouncilDbContext dbContext,
    ICurrentUserService currentUserService,
    IStudentScheduleService studentScheduleService) : PageModel
{
    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        if (!await studentScheduleService.CanAccessScheduleAsync(id, currentUserService.Email ?? string.Empty, currentUserService.Role, cancellationToken))
        {
            return Forbid();
        }

        var schedule = await dbContext.StudentScheduleFiles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (schedule is null)
        {
            return NotFound();
        }

        var path = Path.Combine(studentScheduleService.GetUploadRoot(), schedule.StoredFileName);
        if (!System.IO.File.Exists(path))
        {
            return NotFound();
        }

        var bytes = await System.IO.File.ReadAllBytesAsync(path, cancellationToken);
        return File(bytes, schedule.ContentType, schedule.OriginalFileName);
    }
}
