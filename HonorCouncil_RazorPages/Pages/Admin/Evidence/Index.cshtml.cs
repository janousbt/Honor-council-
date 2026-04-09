using HonorCouncil_RazorPages.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HonorCouncil_RazorPages.Pages.Admin.Evidence;

public class IndexModel(HonorCouncilDbContext dbContext) : PageModel
{
    public IReadOnlyList<EvidenceCaseViewModel> Cases { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Cases = await dbContext.HonorCases
            .AsNoTracking()
            .Include(x => x.Report).ThenInclude(x => x.Student)
            .Include(x => x.Report).ThenInclude(x => x.Course)
            .Include(x => x.EvidenceFiles)
            .Where(x => x.EvidenceFiles.Any())
            .OrderByDescending(x => x.Report.SubmittedUtc)
            .Select(x => new EvidenceCaseViewModel
            {
                CaseId = x.Id,
                CaseNumber = x.CaseNumber,
                StudentName = x.Report.Student.FullName,
                CourseDisplay = $"{x.Report.Course.CourseCode} - {x.Report.Course.CourseName}",
                Files = x.EvidenceFiles
                    .OrderByDescending(file => file.UploadedUtc)
                    .Select(file => new EvidenceFileViewModel
                    {
                        Id = file.Id,
                        FileName = file.OriginalFileName,
                        UploadedBy = file.UploadedByDisplayName,
                        UploadedUtcDisplay = file.UploadedUtc.ToLocalTime().ToString("MMMM d, yyyy h:mm tt")
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);
    }

    public class EvidenceCaseViewModel
    {
        public int CaseId { get; set; }
        public string CaseNumber { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string CourseDisplay { get; set; } = string.Empty;
        public IReadOnlyList<EvidenceFileViewModel> Files { get; set; } = [];
    }

    public class EvidenceFileViewModel
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string UploadedBy { get; set; } = string.Empty;
        public string UploadedUtcDisplay { get; set; } = string.Empty;
    }
}
