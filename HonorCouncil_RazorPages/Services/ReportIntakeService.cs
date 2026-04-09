using HonorCouncil_RazorPages.Data;
using HonorCouncil_RazorPages.Models.Enums;
using HonorCouncil_RazorPages.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HonorCouncil_RazorPages.Services;

public class ReportIntakeService(HonorCouncilDbContext dbContext) : IReportIntakeService
{
    public bool IsFormalReportWithinNinetyDays(DateTime violationDate, DateTime filedDateUtc)
    {
        var violationDateUtc = violationDate.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(violationDate, DateTimeKind.Utc)
            : violationDate.ToUniversalTime();

        return (filedDateUtc.Date - violationDateUtc.Date).TotalDays <= 90;
    }

    public Task<bool> StudentHasPriorQualifyingViolationAsync(string studentNumber, CancellationToken cancellationToken = default)
    {
        return dbContext.Reports
            .Include(x => x.HonorCase)
            .AnyAsync(
                x => x.Student.StudentNumber == studentNumber &&
                     (x.HonorCase == null || x.HonorCase.CurrentStatus != CaseStatus.NoViolation),
                cancellationToken);
    }
}
