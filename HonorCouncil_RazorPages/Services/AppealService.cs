using HonorCouncil_RazorPages.Data;
using HonorCouncil_RazorPages.Models;
using HonorCouncil_RazorPages.Models.Enums;
using HonorCouncil_RazorPages.Options;
using HonorCouncil_RazorPages.Services.Interfaces;
using HonorCouncil_RazorPages.Services.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HonorCouncil_RazorPages.Services;

public class AppealService(
    HonorCouncilDbContext dbContext,
    INotificationService notificationService,
    IAcademicCalendarService academicCalendarService,
    IOptions<NotificationRecipientOptions> notificationRecipientOptions) : IAppealService
{
    private readonly NotificationRecipientOptions _notificationRecipients = notificationRecipientOptions.Value;

    private static readonly IReadOnlyList<AppealStatus> Statuses =
    [
        AppealStatus.Submitted,
        AppealStatus.UnderReview,
        AppealStatus.Approved,
        AppealStatus.Denied
    ];

    public IReadOnlyList<AppealStatus> GetAppealStatuses() => Statuses;

    public async Task<IReadOnlyList<StudentAppealCaseViewModel>> GetStudentAppealCasesAsync(string studentEmail, CancellationToken cancellationToken = default)
    {
        return await dbContext.HonorCases
            .AsNoTracking()
            .Include(x => x.Report).ThenInclude(x => x.Student)
            .Include(x => x.Report).ThenInclude(x => x.Course)
            .Include(x => x.Appeal)
            .Where(x => x.Report.Student.Email == studentEmail && x.Report.ReportType == ReportType.Formal && x.OutcomeIssuedUtc != null)
            .OrderByDescending(x => x.OutcomeIssuedUtc)
            .Select(x => new StudentAppealCaseViewModel
            {
                CaseId = x.Id,
                CaseNumber = x.CaseNumber,
                CourseDisplay = $"{x.Report.Course.CourseCode} - {x.Report.Course.CourseName}",
                OutcomeSummary = x.OutcomeSummary ?? string.Empty,
                OutcomeIssuedUtc = x.OutcomeIssuedUtc,
                CanAppeal = x.Appeal == null,
                HasAppeal = x.Appeal != null,
                AppealStatusDisplay = x.Appeal != null ? x.Appeal.Status.ToString() : string.Empty
            })
            .ToListAsync(cancellationToken);
    }

    public async Task FinalizeOutcomeAsync(int caseId, string outcomeSummary, string performedBy, bool closeCase, CancellationToken cancellationToken = default)
    {
        var honorCase = await dbContext.HonorCases
            .Include(x => x.Report).ThenInclude(x => x.Student)
            .Include(x => x.Report).ThenInclude(x => x.FacultyMember)
            .Include(x => x.StatusTimeline)
            .FirstOrDefaultAsync(x => x.Id == caseId, cancellationToken)
            ?? throw new InvalidOperationException("Case not found.");

        honorCase.OutcomeSummary = outcomeSummary;
        honorCase.OutcomeIssuedUtc = DateTime.UtcNow;
        honorCase.CurrentStatus = closeCase ? CaseStatus.Closed : CaseStatus.OutcomeIssued;
        honorCase.UpdatedUtc = DateTime.UtcNow;
        honorCase.StatusTimeline.Add(new CaseStatusEntry
        {
            Status = CaseStatus.OutcomeIssued,
            Title = closeCase ? "Outcome issued and case closed" : "Outcome issued",
            Notes = outcomeSummary,
            RecordedByDisplayName = performedBy,
            OccurredUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        await notificationService.QueueNotificationAsync(honorCase.Report.Student.Email, $"Outcome issued: {honorCase.CaseNumber}", "OutcomeIssued", honorCase.Id, honorCase.ReportId, cancellationToken);
        await notificationService.QueueNotificationAsync(honorCase.Report.FacultyMember.Email, $"Outcome issued: {honorCase.CaseNumber}", "OutcomeIssued", honorCase.Id, honorCase.ReportId, cancellationToken);
    }

    public async Task<IReadOnlyList<AppealQueueItemViewModel>> GetAppealQueueAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Appeals
            .AsNoTracking()
            .Include(x => x.HonorCase).ThenInclude(x => x.Report).ThenInclude(x => x.Student)
            .OrderByDescending(x => x.SubmittedUtc)
            .Select(x => new AppealQueueItemViewModel
            {
                CaseId = x.HonorCaseId,
                CaseNumber = x.HonorCase.CaseNumber,
                StudentName = x.HonorCase.Report.Student.FullName,
                OutcomeSummary = x.HonorCase.OutcomeSummary ?? string.Empty,
                Status = x.Status,
                StatusDisplay = x.Status.ToString(),
                SubmittedUtc = x.SubmittedUtc
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<StudentAppealPageViewModel?> GetStudentAppealPageAsync(int caseId, string studentEmail, CancellationToken cancellationToken = default)
    {
        return await dbContext.HonorCases
            .AsNoTracking()
            .Include(x => x.Report).ThenInclude(x => x.Student)
            .Include(x => x.Report).ThenInclude(x => x.Course)
            .Include(x => x.Appeal)
            .Where(x => x.Id == caseId && x.Report.Student.Email == studentEmail && x.Report.ReportType == ReportType.Formal)
            .Select(x => new StudentAppealPageViewModel
            {
                CaseId = x.Id,
                CaseNumber = x.CaseNumber,
                StudentName = x.Report.Student.FullName,
                CourseDisplay = $"{x.Report.Course.CourseCode} - {x.Report.Course.CourseName}",
                OutcomeSummary = x.OutcomeSummary ?? string.Empty,
                OutcomeIssuedUtc = x.OutcomeIssuedUtc,
                HasExistingAppeal = x.Appeal != null,
                ExistingAppealStatus = x.Appeal != null ? x.Appeal.Status : null,
                IsAppealWindowEnforced = academicCalendarService.SupportsAppealDeadlineEnforcement,
                AppealWindowMessage = academicCalendarService.GetAppealWindowMessage(x.OutcomeIssuedUtc)
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task SubmitAppealAsync(StudentAppealSubmissionInput input, CancellationToken cancellationToken = default)
    {
        var honorCase = await dbContext.HonorCases
            .Include(x => x.Report).ThenInclude(x => x.Student)
            .Include(x => x.Appeal)
            .Include(x => x.StatusTimeline)
            .FirstOrDefaultAsync(x => x.Id == input.CaseId && x.Report.Student.Email == input.StudentEmail, cancellationToken)
            ?? throw new InvalidOperationException("Case not found.");

        if (honorCase.Report.ReportType != ReportType.Formal || honorCase.OutcomeIssuedUtc is null)
        {
            throw new InvalidOperationException("Appeals are only available for formal cases with an issued outcome.");
        }

        if (honorCase.Appeal is not null)
        {
            throw new InvalidOperationException("An appeal has already been filed for this case.");
        }

        if (academicCalendarService.SupportsAppealDeadlineEnforcement &&
            !academicCalendarService.IsWithinAppealWindow(honorCase.OutcomeIssuedUtc.Value, DateTime.UtcNow))
        {
            throw new InvalidOperationException(academicCalendarService.GetAppealWindowMessage(honorCase.OutcomeIssuedUtc));
        }

        honorCase.Appeal = new Appeal
        {
            Grounds = input.Grounds,
            Description = input.Description,
            Status = AppealStatus.Submitted,
            SubmittedUtc = DateTime.UtcNow
        };
        honorCase.CurrentStatus = CaseStatus.AppealPending;
        honorCase.UpdatedUtc = DateTime.UtcNow;
        honorCase.StatusTimeline.Add(new CaseStatusEntry
        {
            Status = CaseStatus.AppealPending,
            Title = "Appeal submitted",
            Notes = input.Grounds,
            RecordedByDisplayName = honorCase.Report.Student.FullName,
            OccurredUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        await notificationService.QueueNotificationAsync(_notificationRecipients.HonorCouncilEmail, $"Appeal submitted: {honorCase.CaseNumber}", "AppealSubmitted", honorCase.Id, honorCase.ReportId, cancellationToken);
    }

    public async Task<AppealDetailViewModel?> GetAppealDetailAsync(int caseId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Appeals
            .AsNoTracking()
            .Include(x => x.HonorCase).ThenInclude(x => x.Report).ThenInclude(x => x.Student)
            .Include(x => x.HonorCase).ThenInclude(x => x.Report).ThenInclude(x => x.Course)
            .Where(x => x.HonorCaseId == caseId)
            .Select(x => new AppealDetailViewModel
            {
                CaseId = x.HonorCaseId,
                CaseNumber = x.HonorCase.CaseNumber,
                StudentName = x.HonorCase.Report.Student.FullName,
                CourseDisplay = $"{x.HonorCase.Report.Course.CourseCode} - {x.HonorCase.Report.Course.CourseName}",
                OutcomeSummary = x.HonorCase.OutcomeSummary ?? string.Empty,
                OutcomeIssuedUtc = x.HonorCase.OutcomeIssuedUtc,
                Status = x.Status,
                StatusDisplay = x.Status.ToString(),
                SubmittedUtc = x.SubmittedUtc,
                Grounds = x.Grounds,
                Description = x.Description,
                ReviewNotes = x.ReviewNotes
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task ReviewAppealAsync(AdminAppealReviewInput input, string performedBy, CancellationToken cancellationToken = default)
    {
        var appeal = await dbContext.Appeals
            .Include(x => x.HonorCase).ThenInclude(x => x.StatusTimeline)
            .FirstOrDefaultAsync(x => x.HonorCaseId == input.CaseId, cancellationToken)
            ?? throw new InvalidOperationException("Appeal not found.");

        appeal.Status = input.Status;
        appeal.ReviewNotes = input.ReviewNotes;
        appeal.ReviewedUtc = DateTime.UtcNow;
        appeal.HonorCase.CurrentStatus = input.Status == AppealStatus.Approved ? CaseStatus.UnderReview : CaseStatus.Closed;
        appeal.HonorCase.UpdatedUtc = DateTime.UtcNow;
        appeal.HonorCase.StatusTimeline.Add(new CaseStatusEntry
        {
            Status = input.Status == AppealStatus.Approved ? CaseStatus.UnderReview : CaseStatus.Closed,
            Title = $"Appeal {input.Status.ToString().ToLowerInvariant()}",
            Notes = input.ReviewNotes,
            RecordedByDisplayName = performedBy,
            OccurredUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
