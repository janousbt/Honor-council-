using HonorCouncil_RazorPages.Services.Models;
using HonorCouncil_RazorPages.Models.Enums;

namespace HonorCouncil_RazorPages.Services.Interfaces;

public interface IAppealService
{
    IReadOnlyList<AppealStatus> GetAppealStatuses();
    Task FinalizeOutcomeAsync(int caseId, string outcomeSummary, string performedBy, bool closeCase, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AppealQueueItemViewModel>> GetAppealQueueAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StudentAppealCaseViewModel>> GetStudentAppealCasesAsync(string studentEmail, CancellationToken cancellationToken = default);
    Task<StudentAppealPageViewModel?> GetStudentAppealPageAsync(int caseId, string studentEmail, CancellationToken cancellationToken = default);
    Task SubmitAppealAsync(StudentAppealSubmissionInput input, CancellationToken cancellationToken = default);
    Task<AppealDetailViewModel?> GetAppealDetailAsync(int caseId, CancellationToken cancellationToken = default);
    Task ReviewAppealAsync(AdminAppealReviewInput input, string performedBy, CancellationToken cancellationToken = default);
}
