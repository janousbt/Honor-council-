using HonorCouncil_RazorPages.Services.Models;

namespace HonorCouncil_RazorPages.Services.Interfaces;

public interface IStudentCaseService
{
    Task<IReadOnlyList<StudentCaseViewModel>> GetStudentCasesAsync(string studentEmail, CancellationToken cancellationToken = default);
    Task AddWitnessAsync(StudentWitnessSubmissionInput input, CancellationToken cancellationToken = default);
}
