using HonorCouncil_RazorPages.Models.Enums;

namespace HonorCouncil_RazorPages.Services.Models;

public class AppealQueueItemViewModel
{
    public int CaseId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string OutcomeSummary { get; set; } = string.Empty;
    public AppealStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public DateTime SubmittedUtc { get; set; }
}

public class StudentAppealPageViewModel
{
    public int CaseId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string CourseDisplay { get; set; } = string.Empty;
    public string OutcomeSummary { get; set; } = string.Empty;
    public DateTime? OutcomeIssuedUtc { get; set; }
    public bool HasExistingAppeal { get; set; }
    public AppealStatus? ExistingAppealStatus { get; set; }
    public bool IsAppealWindowEnforced { get; set; }
    public string AppealWindowMessage { get; set; } = string.Empty;
}

public class StudentAppealCaseViewModel
{
    public int CaseId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string CourseDisplay { get; set; } = string.Empty;
    public string OutcomeSummary { get; set; } = string.Empty;
    public DateTime? OutcomeIssuedUtc { get; set; }
    public bool CanAppeal { get; set; }
    public bool HasAppeal { get; set; }
    public string AppealStatusDisplay { get; set; } = string.Empty;
}

public class StudentAppealSubmissionInput
{
    public int CaseId { get; set; }
    public string StudentEmail { get; set; } = string.Empty;
    public string Grounds { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class AppealDetailViewModel
{
    public int CaseId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string CourseDisplay { get; set; } = string.Empty;
    public string OutcomeSummary { get; set; } = string.Empty;
    public DateTime? OutcomeIssuedUtc { get; set; }
    public AppealStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public DateTime SubmittedUtc { get; set; }
    public string Grounds { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ReviewNotes { get; set; }
}

public class AdminAppealReviewInput
{
    public int CaseId { get; set; }
    public AppealStatus Status { get; set; }
    public string? ReviewNotes { get; set; }
}
