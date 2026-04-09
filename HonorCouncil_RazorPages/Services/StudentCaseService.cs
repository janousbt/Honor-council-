using HonorCouncil_RazorPages.Data;
using HonorCouncil_RazorPages.Models;
using HonorCouncil_RazorPages.Models.Enums;
using HonorCouncil_RazorPages.Services.Interfaces;
using HonorCouncil_RazorPages.Services.Models;
using Microsoft.EntityFrameworkCore;

namespace HonorCouncil_RazorPages.Services;

public class StudentCaseService(
    HonorCouncilDbContext dbContext,
    ICaseWorkflowService caseWorkflowService) : IStudentCaseService
{
    public async Task<IReadOnlyList<StudentCaseViewModel>> GetStudentCasesAsync(string studentEmail, CancellationToken cancellationToken = default)
    {
        var honorCases = await dbContext.HonorCases
            .AsNoTracking()
            .Include(x => x.Report).ThenInclude(x => x.Course)
            .Include(x => x.Report).ThenInclude(x => x.Student)
            .Include(x => x.Report).ThenInclude(x => x.FacultyMember)
            .Include(x => x.Appeal)
            .Include(x => x.StatusTimeline)
            .Include(x => x.Witnesses)
            .Where(x => x.Report.Student.Email == studentEmail)
            .OrderByDescending(x => x.Report.SubmittedUtc)
            .ToListAsync(cancellationToken);

        return honorCases
            .Select(x => new StudentCaseViewModel
            {
                CaseId = x.Id,
                CaseNumber = x.CaseNumber,
                CourseDisplay = $"{x.Report.Course.CourseCode} - {x.Report.Course.CourseName}",
                ReportType = x.Report.ReportType,
                StatusDisplay = x.CurrentStatus.ToDisplayString(),
                OutcomeSummary = x.OutcomeSummary,
                CanAppeal = x.Report.ReportType == ReportType.Formal && x.OutcomeIssuedUtc != null && x.Appeal == null,
                HasAppeal = x.Appeal != null,
                AppealStatusDisplay = x.Appeal != null ? x.Appeal.Status.ToString() : string.Empty,
                Timeline = caseWorkflowService.GetStudentTimeline(
                    x.Report.ReportType,
                    x.CurrentStatus,
                    x.StatusTimeline.OrderBy(t => t.OccurredUtc).ToList()),
                Witnesses = x.Witnesses
                    .Where(w =>
                        !string.Equals(w.FullName, x.Report.FacultyMember.FullName, StringComparison.OrdinalIgnoreCase) &&
                        (string.IsNullOrWhiteSpace(w.Email) ||
                         !string.Equals(w.Email, x.Report.FacultyMember.Email, StringComparison.OrdinalIgnoreCase)))
                    .OrderBy(w => w.FullName)
                    .Select(w => new StudentWitnessViewModel
                    {
                        CaseNumber = x.CaseNumber,
                        FullName = w.FullName,
                        Email = w.Email,
                        Affiliation = w.Affiliation
                    })
                    .ToList()
            })
            .ToList();
    }

    public async Task AddWitnessAsync(StudentWitnessSubmissionInput input, CancellationToken cancellationToken = default)
    {
        var honorCase = await dbContext.HonorCases
            .Include(x => x.Report).ThenInclude(x => x.Student)
            .FirstOrDefaultAsync(x => x.Id == input.CaseId, cancellationToken)
            ?? throw new InvalidOperationException("Case not found.");

        if (!string.Equals(honorCase.Report.Student.Email, input.StudentEmail.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("You do not have access to add witnesses for this case.");
        }

        var fullName = input.FullName.Trim();
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new InvalidOperationException("Witness name is required.");
        }

        var witness = new Witness
        {
            HonorCaseId = input.CaseId,
            FullName = fullName,
            Email = string.IsNullOrWhiteSpace(input.Email) ? null : input.Email.Trim(),
            Affiliation = string.IsNullOrWhiteSpace(input.Affiliation) ? null : input.Affiliation.Trim()
        };

        dbContext.Witnesses.Add(witness);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
