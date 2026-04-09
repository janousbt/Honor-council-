using Microsoft.AspNetCore.Http;
using HonorCouncil_RazorPages.Services.Models;

namespace HonorCouncil_RazorPages.Services.Interfaces;

public interface IStudentScheduleService
{
    string GetUploadRoot();
    Task RequestClassScheduleAsync(int caseId, string performedBy, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StudentScheduleItemViewModel>> GetStudentSchedulesAsync(string studentEmail, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StudentScheduleItemViewModel>> GetSchedulesForStudentAsync(int studentId, CancellationToken cancellationToken = default);
    Task<int> UploadScheduleAsync(string studentEmail, string uploadedByDisplayName, IFormFile file, CancellationToken cancellationToken = default);
    Task<bool> CanAccessScheduleAsync(int scheduleFileId, string email, string role, CancellationToken cancellationToken = default);
}
