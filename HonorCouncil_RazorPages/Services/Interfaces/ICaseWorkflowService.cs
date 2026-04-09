using HonorCouncil_RazorPages.Models.Enums;
using HonorCouncil_RazorPages.Models;
using HonorCouncil_RazorPages.Services.Models;

namespace HonorCouncil_RazorPages.Services.Interfaces;

public interface ICaseWorkflowService
{
    IReadOnlyList<CaseStatus> GetOrderedTimeline();
    IReadOnlyList<StudentTimelineStepViewModel> GetStudentTimeline(
        ReportType reportType,
        CaseStatus currentStatus,
        IReadOnlyList<CaseStatusEntry> statusEntries);
}
