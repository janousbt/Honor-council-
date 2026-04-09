using HonorCouncil_RazorPages.Data;
using HonorCouncil_RazorPages.Models;
using HonorCouncil_RazorPages.Models.Enums;
using HonorCouncil_RazorPages.Services.Interfaces;
using HonorCouncil_RazorPages.Services.Models;
using Microsoft.EntityFrameworkCore;

namespace HonorCouncil_RazorPages.Services;

public class AdminCaseService(HonorCouncilDbContext dbContext) : IAdminCaseService
{
    public async Task<DashboardSummaryViewModel> GetDashboardSummaryAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var weekEnd = now.Date.AddDays(7);

        var openCases = await dbContext.HonorCases.CountAsync(x => x.CurrentStatus != CaseStatus.Closed && x.CurrentStatus != CaseStatus.NoViolation, cancellationToken);
        var unassignedCases = await dbContext.HonorCases.CountAsync(x => x.AssignedInvestigatorId == null && x.Report.ReportType == ReportType.Formal, cancellationToken);
        var hearingsThisWeek = await dbContext.Hearings.CountAsync(x => x.ScheduledStartUtc >= now.Date && x.ScheduledStartUtc < weekEnd, cancellationToken);
        var pendingAppeals = await dbContext.Appeals.CountAsync(x => x.Status == AppealStatus.Submitted || x.Status == AppealStatus.UnderReview, cancellationToken);

        var priorityCases = await BaseCaseQuery()
            .OrderBy(x =>
                x.Report.Student.AcademicYear != null && EF.Functions.Like(x.Report.Student.AcademicYear, "%Senior%")
                    ? 0
                    : dbContext.Reports.Any(r =>
                        r.StudentId == x.Report.StudentId &&
                        r.Id != x.ReportId &&
                        (r.HonorCase == null || r.HonorCase.CurrentStatus != CaseStatus.NoViolation))
                        ? 1
                        : 2)
            .ThenBy(x => x.Report.SubmittedUtc)
            .Take(5)
            .Select(MapQueueItem())
            .ToListAsync(cancellationToken);

        return new DashboardSummaryViewModel
        {
            OpenCases = openCases,
            UnassignedCases = unassignedCases,
            HearingsThisWeek = hearingsThisWeek,
            PendingAppeals = pendingAppeals,
            PriorityCases = priorityCases
        };
    }

    public async Task<CaseQueueViewModel> GetCaseQueueAsync(string? statusFilter, string? sort, CancellationToken cancellationToken = default)
    {
        var query = BaseCaseQuery();

        if (Enum.TryParse<CaseStatus>(statusFilter, true, out var status))
        {
            query = query.Where(x => x.CurrentStatus == status);
        }

        query = sort?.ToLowerInvariant() switch
        {
            "student" => query.OrderBy(x => x.Report.Student.FullName),
            "course" => query.OrderBy(x => x.Report.Course.CourseCode),
            "status" => query.OrderBy(x => x.CurrentStatus),
            _ => query
                .OrderBy(x =>
                    x.Report.Student.AcademicYear != null && EF.Functions.Like(x.Report.Student.AcademicYear, "%Senior%")
                        ? 0
                        : dbContext.Reports.Any(r =>
                            r.StudentId == x.Report.StudentId &&
                            r.Id != x.ReportId &&
                            (r.HonorCase == null || r.HonorCase.CurrentStatus != CaseStatus.NoViolation))
                            ? 1
                            : 2)
                .ThenBy(x => x.Report.SubmittedUtc)
        };

        var cases = await query.Select(MapQueueItem()).ToListAsync(cancellationToken);
        return new CaseQueueViewModel
        {
            StatusFilter = statusFilter,
            Sort = sort,
            Cases = cases
        };
    }

    public async Task<ArchivedCaseListViewModel> GetArchivedCasesAsync(CancellationToken cancellationToken = default)
    {
        var cases = await dbContext.HonorCases
            .AsNoTracking()
            .Include(x => x.Report).ThenInclude(x => x.Student)
            .Include(x => x.Report).ThenInclude(x => x.Course)
            .Where(x => x.OutcomeIssuedUtc != null || x.CurrentStatus == CaseStatus.Closed || x.CurrentStatus == CaseStatus.NoViolation)
            .OrderByDescending(x => x.OutcomeIssuedUtc ?? x.UpdatedUtc)
            .Select(x => new ArchivedCaseItemViewModel
            {
                CaseId = x.Id,
                CaseNumber = x.CaseNumber,
                StudentName = x.Report.Student.FullName,
                StudentNumber = x.Report.Student.StudentNumber,
                CourseDisplay = $"{x.Report.Course.CourseCode} - {x.Report.Course.CourseName}",
                ReportType = x.Report.ReportType,
                StatusDisplay = x.CurrentStatus.ToDisplayString(),
                OutcomeSummary = x.OutcomeSummary,
                ResolvedUtc = x.OutcomeIssuedUtc ?? x.UpdatedUtc
            })
            .ToListAsync(cancellationToken);

        return new ArchivedCaseListViewModel
        {
            FormalCases = cases.Where(x => x.ReportType == ReportType.Formal).ToList(),
            InformalCases = cases.Where(x => x.ReportType == ReportType.Informal).ToList()
        };
    }

    public async Task<CaseDetailViewModel?> GetCaseDetailAsync(int caseId, CancellationToken cancellationToken = default)
    {
        return await dbContext.HonorCases
            .AsNoTracking()
            .Include(x => x.Report).ThenInclude(x => x.Student)
            .Include(x => x.Report).ThenInclude(x => x.FacultyMember)
            .Include(x => x.Report).ThenInclude(x => x.Course)
            .Include(x => x.Report).ThenInclude(x => x.Student).ThenInclude(x => x.ScheduleFiles)
            .Include(x => x.AssignedInvestigator)
            .Include(x => x.EvidenceFiles)
            .Include(x => x.Witnesses)
            .Include(x => x.StatusTimeline)
            .Include(x => x.Appeal)
            .Where(x => x.Id == caseId)
            .Select(x => new CaseDetailViewModel
            {
                CaseId = x.Id,
                CaseNumber = x.CaseNumber,
                PriorityLabel = x.PriorityLabel ?? "Standard",
                Status = x.CurrentStatus,
                StatusDisplay = x.CurrentStatus.ToDisplayString(),
                InvestigatorName = x.AssignedInvestigator != null ? x.AssignedInvestigator.FullName : null,
                StudentId = x.Report.StudentId,
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
                StudentScheduleFiles = x.Report.Student.ScheduleFiles
                    .OrderByDescending(s => s.UploadedUtc)
                    .Select(s => new StudentScheduleItemViewModel
                    {
                        Id = s.Id,
                        FileName = s.OriginalFileName,
                        UploadedBy = s.UploadedByDisplayName,
                        UploadedUtcDisplay = s.UploadedUtc.ToLocalTime().ToString("MMMM d, yyyy h:mm tt")
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

    public async Task<IReadOnlyList<InvestigatorOptionViewModel>> GetInvestigatorOptionsAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Investigators
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.FullName)
            .Select(x => new InvestigatorOptionViewModel
            {
                Id = x.Id,
                DisplayName = x.FullName
            })
            .ToListAsync(cancellationToken);
    }

    public async Task AssignInvestigatorAsync(int caseId, int investigatorId, string performedBy, CancellationToken cancellationToken = default)
    {
        var honorCase = await dbContext.HonorCases
            .Include(x => x.AssignedInvestigator)
            .Include(x => x.Report)
            .Include(x => x.StatusTimeline)
            .FirstOrDefaultAsync(x => x.Id == caseId, cancellationToken)
            ?? throw new InvalidOperationException("Case not found.");

        if (honorCase.Report.ReportType != ReportType.Formal)
        {
            throw new InvalidOperationException("Investigators can only be assigned to formal violation cases.");
        }

        var investigator = await dbContext.Investigators.FirstOrDefaultAsync(x => x.Id == investigatorId && x.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("Investigator not found.");

        honorCase.AssignedInvestigatorId = investigatorId;
        honorCase.CurrentStatus = CaseStatus.InvestigatorAssigned;
        honorCase.UpdatedUtc = DateTime.UtcNow;
        honorCase.StatusTimeline.Add(new CaseStatusEntry
        {
            Status = CaseStatus.InvestigatorAssigned,
            Title = "Investigator assigned",
            Notes = $"{investigator.FullName} assigned to the case.",
            RecordedByDisplayName = performedBy,
            OccurredUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateCaseStatusAsync(int caseId, string performedBy, string? notes, CaseStatus status, CancellationToken cancellationToken = default)
    {
        var honorCase = await dbContext.HonorCases
            .Include(x => x.StatusTimeline)
            .FirstOrDefaultAsync(x => x.Id == caseId, cancellationToken)
            ?? throw new InvalidOperationException("Case not found.");

        var title = status.ToDisplayString();
        honorCase.CurrentStatus = status;
        honorCase.UpdatedUtc = DateTime.UtcNow;
        honorCase.StatusTimeline.Add(new CaseStatusEntry
        {
            Status = status,
            Title = title,
            Notes = notes,
            RecordedByDisplayName = performedBy,
            OccurredUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<HonorCase> BaseCaseQuery()
    {
        return dbContext.HonorCases
            .AsNoTracking()
            .Include(x => x.Report).ThenInclude(x => x.Student)
            .Include(x => x.Report).ThenInclude(x => x.Course)
            .Include(x => x.AssignedInvestigator);
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
            FiledUtc = x.Report.SubmittedUtc
        };
    }
}
