using HonorCouncil_RazorPages.Services.Models;
using HonorCouncil_RazorPages.Models.Enums;

namespace HonorCouncil_RazorPages.Services.Interfaces;

public interface IAdminCaseService
{
    Task<DashboardSummaryViewModel> GetDashboardSummaryAsync(CancellationToken cancellationToken = default);
    Task<CaseQueueViewModel> GetCaseQueueAsync(string? statusFilter, string? sort, CancellationToken cancellationToken = default);
    Task<ArchivedCaseListViewModel> GetArchivedCasesAsync(CancellationToken cancellationToken = default);
    Task<CaseDetailViewModel?> GetCaseDetailAsync(int caseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InvestigatorOptionViewModel>> GetInvestigatorOptionsAsync(CancellationToken cancellationToken = default);
    Task AssignInvestigatorAsync(int caseId, int investigatorId, string performedBy, CancellationToken cancellationToken = default);
    Task UpdateCaseStatusAsync(int caseId, string performedBy, string? notes, CaseStatus status, CancellationToken cancellationToken = default);
}
