using HonorCouncil_RazorPages.Data;
using HonorCouncil_RazorPages.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HonorCouncil_RazorPages.Pages.Reports;

public class ConfirmationModel(HonorCouncilDbContext dbContext) : PageModel
{
    public string CaseNumber { get; private set; } = string.Empty;
    public string ReportNumber { get; private set; } = string.Empty;
    public ReportType ReportType { get; private set; }
    public string FiledBy { get; private set; } = string.Empty;
    public string StudentDisplay { get; private set; } = string.Empty;
    public string FiledDateDisplay { get; private set; } = string.Empty;
    public string ViolationDateDisplay { get; private set; } = string.Empty;
    public bool IsWithinFormalWindow { get; private set; }
    public IReadOnlyList<string> Recipients { get; private set; } = [];
    public IReadOnlyList<EvidenceFileViewModel> EvidenceFiles { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(string caseNumber, CancellationToken cancellationToken)
    {
        var honorCase = await dbContext.HonorCases
            .Include(x => x.Report)
                .ThenInclude(x => x.Student)
            .Include(x => x.Report)
                .ThenInclude(x => x.FacultyMember)
            .Include(x => x.EvidenceFiles)
            .Include(x => x.Notifications)
            .FirstOrDefaultAsync(x => x.CaseNumber == caseNumber, cancellationToken);

        if (honorCase is null)
        {
            return NotFound();
        }

        CaseNumber = honorCase.CaseNumber;
        ReportNumber = honorCase.Report.ReportNumber;
        ReportType = honorCase.Report.ReportType;
        FiledBy = honorCase.Report.FacultyMember?.FullName ?? "Faculty report";
        StudentDisplay = $"{honorCase.Report.Student.FullName} ({honorCase.Report.Student.StudentNumber})";
        FiledDateDisplay = honorCase.Report.SubmittedUtc.ToLocalTime().ToString("MMMM d, yyyy");
        ViolationDateDisplay = honorCase.Report.ViolationDate.ToString("MMMM d, yyyy");
        IsWithinFormalWindow = honorCase.Report.IsWithinFormalFilingWindow;
        Recipients = honorCase.Notifications.Select(x => x.RecipientEmail).Distinct().ToList();
        EvidenceFiles = honorCase.EvidenceFiles
            .Select(x => new EvidenceFileViewModel { Id = x.Id, FileName = x.OriginalFileName })
            .ToList();

        return Page();
    }

    public class EvidenceFileViewModel
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
    }
}
