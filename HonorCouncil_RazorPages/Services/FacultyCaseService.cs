using HonorCouncil_RazorPages.Data;
using HonorCouncil_RazorPages.Models;
using HonorCouncil_RazorPages.Models.Enums;
using HonorCouncil_RazorPages.Services.Interfaces;
using HonorCouncil_RazorPages.Services.Models;
using Microsoft.EntityFrameworkCore;

namespace HonorCouncil_RazorPages.Services;

public class FacultyCaseService(HonorCouncilDbContext dbContext) : IFacultyCaseService
{
    public async Task<FacultyDashboardSummaryViewModel> GetDashboardSummaryAsync(string facultyEmail, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var weekEnd = now.Date.AddDays(7);
        var caseIds = await BaseCaseQuery(facultyEmail)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var openCases = await BaseCaseQuery(facultyEmail)
            .CountAsync(x => x.CurrentStatus != CaseStatus.Closed && x.CurrentStatus != CaseStatus.NoViolation, cancellationToken);

        var hearingsThisWeek = await BaseCaseQuery(facultyEmail)
            .CountAsync(x => x.Hearing != null && x.Hearing.ScheduledStartUtc >= now.Date && x.Hearing.ScheduledStartUtc < weekEnd, cancellationToken);

        var pendingAppeals = await BaseCaseQuery(facultyEmail)
            .CountAsync(x => x.Appeal != null && (x.Appeal.Status == AppealStatus.Submitted || x.Appeal.Status == AppealStatus.UnderReview), cancellationToken);

        var evidenceFiles = await dbContext.EvidenceFiles
            .AsNoTracking()
            .CountAsync(x => caseIds.Contains(x.HonorCaseId), cancellationToken);

        var cases = await BaseCaseQuery(facultyEmail)
            .OrderByDescending(x => x.Report.SubmittedUtc)
            .Take(5)
            .Select(MapQueueItem())
            .ToListAsync(cancellationToken);

        var recentEvidence = await dbContext.EvidenceFiles
            .AsNoTracking()
            .Where(x => caseIds.Contains(x.HonorCaseId))
            .OrderByDescending(x => x.UploadedUtc)
            .Take(5)
            .Select(x => new FacultyEvidenceItemViewModel
            {
                Id = x.Id,
                CaseId = x.HonorCaseId,
                CaseNumber = x.HonorCase.CaseNumber,
                FileName = x.OriginalFileName,
                UploadedBy = x.UploadedByDisplayName
            })
            .ToListAsync(cancellationToken);

        return new FacultyDashboardSummaryViewModel
        {
            OpenCases = openCases,
            EvidenceFiles = evidenceFiles,
            HearingsThisWeek = hearingsThisWeek,
            PendingAppeals = pendingAppeals,
            Cases = cases,
            RecentEvidence = recentEvidence
        };
    }

    public async Task<CaseDetailViewModel?> GetCaseDetailAsync(int caseId, string facultyEmail, CancellationToken cancellationToken = default)
    {
        return await dbContext.HonorCases
            .AsNoTracking()
            .Include(x => x.Report).ThenInclude(x => x.Student)
            .Include(x => x.Report).ThenInclude(x => x.FacultyMember)
            .Include(x => x.Report).ThenInclude(x => x.Course)
            .Include(x => x.AssignedInvestigator)
            .Include(x => x.EvidenceFiles)
            .Include(x => x.Witnesses)
            .Include(x => x.StatusTimeline)
            .Include(x => x.Appeal)
            .Where(x => x.Id == caseId && x.Report.FacultyMember.Email == facultyEmail)
            .Select(x => new CaseDetailViewModel
            {
                CaseId = x.Id,
                CaseNumber = x.CaseNumber,
                PriorityLabel = x.PriorityLabel ?? "Standard",
                Status = x.CurrentStatus,
                StatusDisplay = x.CurrentStatus.ToDisplayString(),
                InvestigatorName = x.AssignedInvestigator != null ? x.AssignedInvestigator.FullName : null,
                StudentName = x.Report.Student.FullName,
                StudentNumber = x.Report.Student.StudentNumber,
                StudentEmail = x.Report.Student.Email,
                StudentAcademicYear = x.Report.Student.AcademicYear,
                FacultyName = x.Report.FacultyMember.FullName,
                FacultyEmail = x.Report.FacultyMember.Email,
                FacultyDepartment = x.Report.FacultyMember.Department,
                CourseDisplay = $"{x.Report.Course.CourseCode} - {x.Report.Course.CourseName}",
                ViolationDate = x.Report.ViolationDate,
                FiledUtc = x.Report.SubmittedUtc,
                IsWithinFormalWindow = x.Report.IsWithinFormalFilingWindow,
                ReportType = x.Report.ReportType,
                Description = x.Report.ViolationDescription,
                ScheduledStartUtc = x.Hearing != null ? x.Hearing.ScheduledStartUtc : null,
                HearingFormat = x.Hearing != null ? x.Hearing.HearingFormat : null,
                LocationOrMeetingLink = x.Hearing != null ? x.Hearing.LocationOrMeetingLink : null,
                OutcomeSummary = x.OutcomeSummary,
                HasAppeal = x.Appeal != null,
                AppealStatusDisplay = x.Appeal != null ? x.Appeal.Status.ToString() : "No appeal",
                EvidenceFiles = x.EvidenceFiles
                    .OrderBy(e => e.OriginalFileName)
                    .Select(e => new EvidenceItemViewModel
                    {
                        Id = e.Id,
                        FileName = e.OriginalFileName,
                        UploadedBy = e.UploadedByDisplayName,
                        UploadedByRole = e.UploadedByRole,
                        UploadedUtcDisplay = e.UploadedUtc.ToLocalTime().ToString("MMMM d, yyyy h:mm tt")
                    }).ToList(),
                Witnesses = x.Witnesses
                    .OrderBy(w => w.FullName)
                    .Select(w => new WitnessItemViewModel
                    {
                        FullName = w.FullName,
                        Email = w.Email,
                        Affiliation = w.Affiliation
                    }).ToList(),
                Timeline = x.StatusTimeline
                    .OrderBy(t => t.OccurredUtc)
                    .Select(t => new TimelineItemViewModel
                    {
                        Title = t.Title,
                        WhenDisplay = t.OccurredUtc.ToLocalTime().ToString("MMMM d, yyyy"),
                        Notes = t.Notes
                    }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private IQueryable<HonorCase> BaseCaseQuery(string facultyEmail)
    {
        return dbContext.HonorCases
            .AsNoTracking()
            .Include(x => x.Report).ThenInclude(x => x.Student)
            .Include(x => x.Report).ThenInclude(x => x.Course)
            .Include(x => x.AssignedInvestigator)
            .Include(x => x.Hearing)
            .Include(x => x.Appeal)
            .Where(x => x.Report.FacultyMember.Email == facultyEmail);
    }

    private static System.Linq.Expressions.Expression<Func<HonorCase, CaseQueueItemViewModel>> MapQueueItem()
    {
        return x => new CaseQueueItemViewModel
        {
            CaseId = x.Id,
            CaseNumber = x.CaseNumber,
            StudentName = x.Report.Student.FullName,
            StudentNumber = x.Report.Student.StudentNumber,
            AcademicYear = x.Report.Student.AcademicYear,
            CourseNumber = x.Report.Course.CourseCode,
            CourseTitle = x.Report.Course.CourseName,
            ReportType = x.Report.ReportType,
            PriorityLabel = x.PriorityLabel ?? "Standard",
            Status = x.CurrentStatus,
            StatusDisplay = x.CurrentStatus.ToDisplayString(),
            InvestigatorName = x.AssignedInvestigator != null ? x.AssignedInvestigator.FullName : null,
            FiledUtc = x.Report.SubmittedUtc,
            ScheduledStartUtc = x.Hearing != null ? x.Hearing.ScheduledStartUtc : null,
            HearingFormat = x.Hearing != null ? x.Hearing.HearingFormat : null
        };
    }
}
