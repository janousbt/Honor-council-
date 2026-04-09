using HonorCouncil_RazorPages.Data;
using HonorCouncil_RazorPages.Models;
using HonorCouncil_RazorPages.Models.Enums;
using HonorCouncil_RazorPages.Options;
using HonorCouncil_RazorPages.Services.Interfaces;
using HonorCouncil_RazorPages.Services.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HonorCouncil_RazorPages.Services;

public class ReportSubmissionService(
    HonorCouncilDbContext dbContext,
    IReportIntakeService reportIntakeService,
    IEvidenceService evidenceService,
    INotificationService notificationService,
    IOptions<NotificationRecipientOptions> recipientOptions) : IReportSubmissionService
{
    private readonly NotificationRecipientOptions _recipientOptions = recipientOptions.Value;

    public async Task<SubmittedReportResult> SubmitFormalReportAsync(ReportSubmissionInput input, IReadOnlyList<IFormFile> evidenceFiles, CancellationToken cancellationToken = default)
    {
        var filedUtc = DateTime.UtcNow;
        var isWithinWindow = reportIntakeService.IsFormalReportWithinNinetyDays(input.ViolationDate, filedUtc);
        if (!isWithinWindow)
        {
            throw new InvalidOperationException("Formal reports must be filed within 90 days of the violation date.");
        }

        var hasPriorViolation = await reportIntakeService.StudentHasPriorQualifyingViolationAsync(input.StudentNumber, cancellationToken);
        return await SubmitReportAsync(input, ReportType.Formal, isWithinWindow, hasPriorViolation, evidenceFiles, cancellationToken);
    }

    public async Task<SubmittedReportResult> SubmitInformalReportAsync(ReportSubmissionInput input, CancellationToken cancellationToken = default)
    {
        var hasPriorViolation = await reportIntakeService.StudentHasPriorQualifyingViolationAsync(input.StudentNumber, cancellationToken);
        if (hasPriorViolation)
        {
            throw new InvalidOperationException("This student already has a prior qualifying violation. File a formal report instead.");
        }

        return await SubmitReportAsync(input, ReportType.Informal, true, false, [], cancellationToken);
    }

    private async Task<SubmittedReportResult> SubmitReportAsync(
        ReportSubmissionInput input,
        ReportType reportType,
        bool isWithinWindow,
        bool hasPriorViolation,
        IReadOnlyList<IFormFile> evidenceFiles,
        CancellationToken cancellationToken)
    {
        var student = await GetOrCreateStudentAsync(input, cancellationToken);
        var faculty = await GetOrCreateFacultyAsync(input, cancellationToken);
        var course = await GetOrCreateCourseAsync(input, cancellationToken);

        var reportNumber = GenerateNumber("HR");
        var caseNumber = GenerateNumber("HC");
        var filedUtc = DateTime.UtcNow;
        var hasEvidence = evidenceFiles.Any(x => x.Length > 0);

        var report = new Report
        {
            ReportNumber = reportNumber,
            ReportType = reportType,
            SubmittedUtc = filedUtc,
            ViolationDate = input.ViolationDate,
            ViolationDescription = input.Description,
            IsWithinFormalFilingWindow = isWithinWindow,
            WasRedirectedToFormal = input.WasRedirectedToFormal,
            SubmissionNotes = input.SubmissionNotes,
            Student = student,
            FacultyMember = faculty,
            Course = course
        };

        var honorCase = new HonorCase
        {
            CaseNumber = caseNumber,
            CurrentStatus = reportType == ReportType.Formal ? CaseStatus.Unassigned : CaseStatus.UnderReview,
            PriorityLabel = BuildPriorityLabel(input.StudentAcademicYear, hasPriorViolation, reportType),
            Report = report,
            StatusTimeline =
            {
                new CaseStatusEntry
                {
                    Status = CaseStatus.ReportReceived,
                    Title = "Report received",
                    Notes = reportType switch
                    {
                        ReportType.Formal when hasEvidence => "Formal report submitted with evidence.",
                        ReportType.Formal => "Formal report submitted.",
                        _ => "Informal report submitted."
                    },
                    RecordedByDisplayName = input.FacultyName,
                    OccurredUtc = filedUtc
                }
            }
        };

        foreach (var witnessInput in input.Witnesses.Where(x => !string.IsNullOrWhiteSpace(x.FullName)))
        {
            honorCase.Witnesses.Add(new Witness
            {
                FullName = witnessInput.FullName.Trim(),
                Email = string.IsNullOrWhiteSpace(witnessInput.Email) ? null : witnessInput.Email.Trim(),
                Affiliation = string.IsNullOrWhiteSpace(witnessInput.Affiliation) ? null : witnessInput.Affiliation.Trim()
            });
        }

        dbContext.HonorCases.Add(honorCase);
        await dbContext.SaveChangesAsync(cancellationToken);
        await EnsureCourseFacultyAssignmentAsync(course, faculty, cancellationToken);

        var evidenceIds = new List<int>();
        if (reportType == ReportType.Formal)
        {
            evidenceIds = await SaveEvidenceFilesAsync(honorCase, evidenceFiles, cancellationToken);
        }

        var recipients = GetRecipients(reportType);
        foreach (var recipient in recipients)
        {
            await notificationService.QueueNotificationAsync(
                recipient,
                $"{reportType} report submitted: {caseNumber}",
                reportType == ReportType.Formal ? "FormalReportSubmission" : "InformalReportSubmission",
                honorCase.Id,
                report.Id,
                cancellationToken);
        }

        return new SubmittedReportResult
        {
            CaseNumber = caseNumber,
            ReportNumber = reportNumber,
            ReportType = reportType,
            FiledBy = input.FacultyName,
            StudentName = input.StudentName,
            StudentNumber = input.StudentNumber,
            FiledUtc = filedUtc,
            ViolationDate = input.ViolationDate,
            IsWithinFormalFilingWindow = isWithinWindow,
            WasRedirectedToFormal = input.WasRedirectedToFormal,
            Recipients = recipients,
            EvidenceIds = evidenceIds
        };
    }

    private async Task<Student> GetOrCreateStudentAsync(ReportSubmissionInput input, CancellationToken cancellationToken)
    {
        var student = await dbContext.Students.FirstOrDefaultAsync(x => x.StudentNumber == input.StudentNumber, cancellationToken);
        if (student is null)
        {
            student = new Student { StudentNumber = input.StudentNumber };
            dbContext.Students.Add(student);
        }

        student.FullName = input.StudentName.Trim();
        student.Email = input.StudentEmail.Trim();
        student.Major = string.IsNullOrWhiteSpace(input.StudentMajor) ? null : input.StudentMajor.Trim();
        student.AcademicYear = string.IsNullOrWhiteSpace(input.StudentAcademicYear) ? null : input.StudentAcademicYear.Trim();
        return student;
    }

    private async Task<FacultyMember> GetOrCreateFacultyAsync(ReportSubmissionInput input, CancellationToken cancellationToken)
    {
        var faculty = await dbContext.FacultyMembers.FirstOrDefaultAsync(x => x.Email == input.FacultyEmail, cancellationToken);
        if (faculty is null)
        {
            faculty = new FacultyMember { Email = input.FacultyEmail.Trim() };
            dbContext.FacultyMembers.Add(faculty);
        }

        faculty.FullName = input.FacultyName.Trim();
        faculty.Department = string.IsNullOrWhiteSpace(input.FacultyDepartment) ? null : input.FacultyDepartment.Trim();
        return faculty;
    }

    private async Task<Course> GetOrCreateCourseAsync(ReportSubmissionInput input, CancellationToken cancellationToken)
    {
        var course = await dbContext.Courses.FirstOrDefaultAsync(
            x => x.CourseCode == input.CourseCode && x.Section == input.CourseSection && x.Term == input.Term,
            cancellationToken);

        if (course is null)
        {
            course = new Course();
            dbContext.Courses.Add(course);
        }

        course.CourseCode = input.CourseCode.Trim();
        course.CourseName = input.CourseName.Trim();
        course.Section = string.IsNullOrWhiteSpace(input.CourseSection) ? null : input.CourseSection.Trim();
        course.Term = string.IsNullOrWhiteSpace(input.Term) ? null : input.Term.Trim();
        return course;
    }

    private async Task<List<int>> SaveEvidenceFilesAsync(HonorCase honorCase, IReadOnlyList<IFormFile> evidenceFiles, CancellationToken cancellationToken)
    {
        var caseFolder = Path.Combine(evidenceService.GetUploadRoot(), honorCase.CaseNumber);
        Directory.CreateDirectory(caseFolder);

        var evidenceIds = new List<int>();
        foreach (var file in evidenceFiles.Where(x => x.Length > 0))
        {
            var extension = Path.GetExtension(file.FileName);
            var storedFileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(caseFolder, storedFileName);

            await using var stream = File.Create(fullPath);
            await file.CopyToAsync(stream, cancellationToken);

            var evidence = new EvidenceFile
            {
                HonorCaseId = honorCase.Id,
                OriginalFileName = Path.GetFileName(file.FileName),
                StoredFileName = Path.Combine(honorCase.CaseNumber, storedFileName),
                ContentType = file.ContentType,
                FileSizeBytes = file.Length,
                UploadedByDisplayName = honorCase.Report.FacultyMember.FullName,
                UploadedByRole = "Faculty"
            };

            dbContext.EvidenceFiles.Add(evidence);
            await dbContext.SaveChangesAsync(cancellationToken);
            evidenceIds.Add(evidence.Id);
        }

        return evidenceIds;
    }

    private async Task EnsureCourseFacultyAssignmentAsync(Course course, FacultyMember faculty, CancellationToken cancellationToken)
    {
        var existingAssignment = await dbContext.CourseFacultyAssignments
            .AnyAsync(x => x.CourseId == course.Id && x.FacultyMemberId == faculty.Id, cancellationToken);

        if (existingAssignment)
        {
            return;
        }

        dbContext.CourseFacultyAssignments.Add(new CourseFaculty
        {
            CourseId = course.Id,
            FacultyMemberId = faculty.Id
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private IReadOnlyList<string> GetRecipients(ReportType reportType)
    {
        return reportType == ReportType.Formal
            ? [_recipientOptions.HonorCouncilEmail, _recipientOptions.PresidentEmail, _recipientOptions.CoordinatorEmail]
            : [_recipientOptions.HonorCouncilEmail];
    }

    private static string BuildPriorityLabel(string? academicYear, bool hasPriorViolation, ReportType reportType)
    {
        var isSenior = !string.IsNullOrWhiteSpace(academicYear) &&
                       academicYear.Contains("senior", StringComparison.OrdinalIgnoreCase);

        if (isSenior && hasPriorViolation)
        {
            return "Senior second violation";
        }

        if (isSenior)
        {
            return "Senior priority";
        }

        if (hasPriorViolation)
        {
            return "Second violation priority";
        }

        return reportType == ReportType.Formal ? "Formal review" : "Informal";
    }

    private static string GenerateNumber(string prefix) => $"{prefix}-{DateTime.UtcNow:yyyy}-{Random.Shared.Next(1000, 9999)}";
}
