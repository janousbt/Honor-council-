using HonorCouncil_RazorPages.Data;
using HonorCouncil_RazorPages.Models;
using HonorCouncil_RazorPages.Services.Interfaces;
using HonorCouncil_RazorPages.Services.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HonorCouncil_RazorPages.Services;

public class StudentScheduleService(
    HonorCouncilDbContext dbContext,
    IWebHostEnvironment environment,
    INotificationService notificationService) : IStudentScheduleService
{
    public string GetUploadRoot() => Path.Combine(environment.ContentRootPath, "App_Data", "Schedules");

    public async Task RequestClassScheduleAsync(int caseId, string performedBy, CancellationToken cancellationToken = default)
    {
        var honorCase = await dbContext.HonorCases
            .Include(x => x.Report).ThenInclude(x => x.Student)
            .Include(x => x.StatusTimeline)
            .FirstOrDefaultAsync(x => x.Id == caseId, cancellationToken)
            ?? throw new InvalidOperationException("Case not found.");

        honorCase.StatusTimeline.Add(new CaseStatusEntry
        {
            Status = honorCase.CurrentStatus,
            Title = "Class schedule requested",
            Notes = "Student was asked to upload their class schedule through the student account.",
            RecordedByDisplayName = performedBy,
            OccurredUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        await notificationService.QueueNotificationAsync(
            honorCase.Report.Student.Email,
            $"Class schedule requested: {honorCase.CaseNumber}",
            "ClassScheduleRequest",
            honorCase.Id,
            honorCase.ReportId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<StudentScheduleItemViewModel>> GetStudentSchedulesAsync(string studentEmail, CancellationToken cancellationToken = default)
    {
        return await dbContext.StudentScheduleFiles
            .AsNoTracking()
            .Where(x => x.Student.Email == studentEmail)
            .OrderByDescending(x => x.UploadedUtc)
            .Select(x => new StudentScheduleItemViewModel
            {
                Id = x.Id,
                FileName = x.OriginalFileName,
                UploadedBy = x.UploadedByDisplayName,
                UploadedUtcDisplay = x.UploadedUtc.ToLocalTime().ToString("MMMM d, yyyy h:mm tt")
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StudentScheduleItemViewModel>> GetSchedulesForStudentAsync(int studentId, CancellationToken cancellationToken = default)
    {
        return await dbContext.StudentScheduleFiles
            .AsNoTracking()
            .Where(x => x.StudentId == studentId)
            .OrderByDescending(x => x.UploadedUtc)
            .Select(x => new StudentScheduleItemViewModel
            {
                Id = x.Id,
                FileName = x.OriginalFileName,
                UploadedBy = x.UploadedByDisplayName,
                UploadedUtcDisplay = x.UploadedUtc.ToLocalTime().ToString("MMMM d, yyyy h:mm tt")
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<int> UploadScheduleAsync(string studentEmail, string uploadedByDisplayName, IFormFile file, CancellationToken cancellationToken = default)
    {
        var student = await dbContext.Students.FirstOrDefaultAsync(x => x.Email == studentEmail, cancellationToken)
            ?? throw new InvalidOperationException("Student record not found.");

        if (file.Length == 0)
        {
            throw new InvalidOperationException("Select a schedule file to upload.");
        }

        var studentFolder = Path.Combine(GetUploadRoot(), student.StudentNumber);
        Directory.CreateDirectory(studentFolder);

        var extension = Path.GetExtension(file.FileName);
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var relativePath = Path.Combine(student.StudentNumber, storedFileName);
        var fullPath = Path.Combine(studentFolder, storedFileName);

        await using var stream = File.Create(fullPath);
        await file.CopyToAsync(stream, cancellationToken);

        var scheduleFile = new StudentScheduleFile
        {
            StudentId = student.Id,
            OriginalFileName = Path.GetFileName(file.FileName),
            StoredFileName = relativePath,
            ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
            FileSizeBytes = file.Length,
            UploadedByDisplayName = uploadedByDisplayName
        };

        dbContext.StudentScheduleFiles.Add(scheduleFile);
        await dbContext.SaveChangesAsync(cancellationToken);
        return scheduleFile.Id;
    }

    public async Task<bool> CanAccessScheduleAsync(int scheduleFileId, string email, string role, CancellationToken cancellationToken = default)
    {
        var file = await dbContext.StudentScheduleFiles
            .AsNoTracking()
            .Include(x => x.Student)
            .FirstOrDefaultAsync(x => x.Id == scheduleFileId, cancellationToken);

        if (file is null)
        {
            return false;
        }

        if (string.Equals(role, "Student", StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(file.Student.Email, email, StringComparison.OrdinalIgnoreCase);
        }

        return role is "Admin" or "President" or "Coordinator" or "Investigator";
    }
}
