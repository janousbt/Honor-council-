using HonorCouncil_RazorPages.Services.Models;
using HonorCouncil_RazorPages.Models.Enums;

namespace HonorCouncil_RazorPages.Services.Interfaces;

public interface IHearingService
{
    IReadOnlyList<HearingFormat> GetSupportedFormats();
    Task<HearingQueueViewModel> GetHearingQueueAsync(CancellationToken cancellationToken = default);
    Task<HearingScheduleViewModel?> GetScheduleDetailsAsync(int caseId, CancellationToken cancellationToken = default);
    Task ScheduleHearingAsync(HearingScheduleInput input, string performedBy, CancellationToken cancellationToken = default);
    Task RequestAvailabilityAsync(int caseId, string performedBy, CancellationToken cancellationToken = default);
    Task SubmitAvailabilityAsync(AvailabilitySubmissionInput input, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParticipantCaseOptionViewModel>> GetStudentCasesAsync(string studentEmail, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParticipantCaseOptionViewModel>> GetFacultyCasesAsync(string facultyEmail, CancellationToken cancellationToken = default);
}
