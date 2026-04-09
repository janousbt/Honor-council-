namespace HonorCouncil_RazorPages.Services.Models;

public class FacultyDashboardSummaryViewModel
{
    public int OpenCases { get; set; }
    public int EvidenceFiles { get; set; }
    public int HearingsThisWeek { get; set; }
    public int PendingAppeals { get; set; }
    public IReadOnlyList<CaseQueueItemViewModel> Cases { get; set; } = [];
    public IReadOnlyList<FacultyEvidenceItemViewModel> RecentEvidence { get; set; } = [];
}

public class FacultyEvidenceItemViewModel
{
    public int Id { get; set; }
    public int CaseId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
}
