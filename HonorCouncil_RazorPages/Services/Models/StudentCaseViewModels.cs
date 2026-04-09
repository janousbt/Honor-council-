using HonorCouncil_RazorPages.Models.Enums;

namespace HonorCouncil_RazorPages.Services.Models;

public class StudentCaseViewModel
{
    public int CaseId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string CourseDisplay { get; set; } = string.Empty;
    public ReportType ReportType { get; set; }
    public string ReportTypeDisplay => ReportType == ReportType.Formal ? "Formal violation" : "Informal resolution";
    public string StatusDisplay { get; set; } = string.Empty;
    public string? OutcomeSummary { get; set; }
    public bool CanAppeal { get; set; }
    public bool HasAppeal { get; set; }
    public string AppealStatusDisplay { get; set; } = string.Empty;
    public IReadOnlyList<StudentTimelineStepViewModel> Timeline { get; set; } = [];
    public IReadOnlyList<StudentWitnessViewModel> Witnesses { get; set; } = [];
}

public class StudentTimelineStepViewModel
{
    public string Title { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public string State { get; set; } = "pending";
}

public class StudentWitnessViewModel
{
    public string CaseNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Affiliation { get; set; }
}

public class StudentWitnessSubmissionInput
{
    public int CaseId { get; set; }
    public string StudentEmail { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Affiliation { get; set; }
}
