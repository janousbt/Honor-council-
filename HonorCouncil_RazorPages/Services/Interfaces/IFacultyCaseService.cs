using HonorCouncil_RazorPages.Services.Models;

namespace HonorCouncil_RazorPages.Services.Interfaces;

public interface IFacultyCaseService
{
    Task<FacultyDashboardSummaryViewModel> GetDashboardSummaryAsync(string facultyEmail, CancellationToken cancellationToken = default);
    Task<CaseDetailViewModel?> GetCaseDetailAsync(int caseId, string facultyEmail, CancellationToken cancellationToken = default);
}
