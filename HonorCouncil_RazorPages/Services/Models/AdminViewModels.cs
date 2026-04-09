using HonorCouncil_RazorPages.Models.Enums;

namespace HonorCouncil_RazorPages.Services.Models;

public class DashboardSummaryViewModel
{
    public int OpenCases { get; set; }
    public int UnassignedCases { get; set; }
    public int HearingsThisWeek { get; set; }
    public int PendingAppeals { get; set; }
    public IReadOnlyList<CaseQueueItemViewModel> PriorityCases { get; set; } = [];
}

public class CaseQueueViewModel
{
    public string? StatusFilter { get; set; }
    public string? Sort { get; set; }
    public IReadOnlyList<CaseQueueItemViewModel> Cases { get; set; } = [];
}

public class CaseQueueItemViewModel
{
    public int CaseId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public string? AcademicYear { get; set; }
    public string CourseNumber { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public ReportType ReportType { get; set; }
    public string PriorityLabel { get; set; } = string.Empty;
    public CaseStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public string? InvestigatorName { get; set; }
    public DateTime FiledUtc { get; set; }
    public DateTime? ScheduledStartUtc { get; set; }
    public HearingFormat? HearingFormat { get; set; }
}

public class CaseDetailViewModel
{
    public int CaseId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string PriorityLabel { get; set; } = string.Empty;
    public CaseStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public string? InvestigatorName { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public string? StudentAcademicYear { get; set; }
    public string FacultyName { get; set; } = string.Empty;
    public string FacultyEmail { get; set; } = string.Empty;
    public string? FacultyDepartment { get; set; }
    public string CourseDisplay { get; set; } = string.Empty;
    public DateTime ViolationDate { get; set; }
    public DateTime FiledUtc { get; set; }
    public bool IsWithinFormalWindow { get; set; }
    public ReportType ReportType { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime? ScheduledStartUtc { get; set; }
    public HearingFormat? HearingFormat { get; set; }
    public string? LocationOrMeetingLink { get; set; }
    public string? OutcomeSummary { get; set; }
    public bool HasAppeal { get; set; }
    public string AppealStatusDisplay { get; set; } = "No appeal";
    public IReadOnlyList<EvidenceItemViewModel> EvidenceFiles { get; set; } = [];
    public IReadOnlyList<StudentScheduleItemViewModel> StudentScheduleFiles { get; set; } = [];
    public IReadOnlyList<WitnessItemViewModel> Witnesses { get; set; } = [];
    public IReadOnlyList<TimelineItemViewModel> Timeline { get; set; } = [];
}

public class ArchivedCaseListViewModel
{
    public IReadOnlyList<ArchivedCaseItemViewModel> FormalCases { get; set; } = [];
    public IReadOnlyList<ArchivedCaseItemViewModel> InformalCases { get; set; } = [];
}

public class ArchivedCaseItemViewModel
{
    public int CaseId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public string CourseDisplay { get; set; } = string.Empty;
    public ReportType ReportType { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public string? OutcomeSummary { get; set; }
    public DateTime ResolvedUtc { get; set; }
}

public class EvidenceItemViewModel
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public string UploadedByRole { get; set; } = string.Empty;
    public string UploadedUtcDisplay { get; set; } = string.Empty;
}

public class WitnessItemViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Affiliation { get; set; }
}

public class TimelineItemViewModel
{
    public string Title { get; set; } = string.Empty;
    public string WhenDisplay { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class InvestigatorOptionViewModel
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}
