using HonorCouncil_RazorPages.Services.Models;
using Microsoft.AspNetCore.Http;

namespace HonorCouncil_RazorPages.Services.Interfaces;

public interface IReportSubmissionService
{
    Task<SubmittedReportResult> SubmitFormalReportAsync(ReportSubmissionInput input, IReadOnlyList<IFormFile> evidenceFiles, CancellationToken cancellationToken = default);
    Task<SubmittedReportResult> SubmitInformalReportAsync(ReportSubmissionInput input, CancellationToken cancellationToken = default);
}
