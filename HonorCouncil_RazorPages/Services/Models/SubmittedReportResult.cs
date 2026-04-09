using HonorCouncil_RazorPages.Models.Enums;

namespace HonorCouncil_RazorPages.Services.Models;

public class SubmittedReportResult
{
    public string CaseNumber { get; set; } = string.Empty;
    public string ReportNumber { get; set; } = string.Empty;
    public ReportType ReportType { get; set; }
    public string FiledBy { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public DateTime FiledUtc { get; set; }
    public DateTime ViolationDate { get; set; }
    public bool IsWithinFormalFilingWindow { get; set; }
    public bool WasRedirectedToFormal { get; set; }
    public IReadOnlyList<string> Recipients { get; set; } = [];
    public IReadOnlyList<int> EvidenceIds { get; set; } = [];
}
