using HonorCouncil_RazorPages.Data;
using HonorCouncil_RazorPages.Models;
using HonorCouncil_RazorPages.Models.Enums;
using HonorCouncil_RazorPages.Options;
using HonorCouncil_RazorPages.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HonorCouncil_RazorPages.Services;

public class DatabaseInitializer(
    HonorCouncilDbContext dbContext,
    IEvidenceService evidenceService,
    IStudentScheduleService studentScheduleService,
    IWebHostEnvironment environment,
    IPasswordHasher<ApplicationUser> passwordHasher,
    IOptions<SeedUserOptions> seedUserOptions) : IDatabaseInitializer
{
    private readonly SeedUserOptions _seedUserOptions = seedUserOptions.Value;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);
        Directory.CreateDirectory(evidenceService.GetUploadRoot());
        Directory.CreateDirectory(studentScheduleService.GetUploadRoot());
        await SeedApplicationUsersAsync(cancellationToken);
        await NormalizeSampleCourseDataAsync(cancellationToken);

        if (!environment.IsDevelopment() || await dbContext.Reports.AnyAsync(cancellationToken))
        {
            return;
        }

        var faculty = new FacultyMember
        {
            FullName = "Dr. Peter Parker",
            Email = "pparker@jmu.edu",
            Department = "Computer Information Systems"
        };

        var student = new Student
        {
            FullName = "John Doe",
            StudentNumber = "110435980",
            Email = "jdoe@dukes.jmu.edu",
            AcademicYear = "Senior",
            Major = "Computer Information Systems"
        };

        var course = new Course
        {
            CourseCode = "CIS 484",
            CourseName = "CIS 484",
            Section = "001",
            Term = "Spring 2026"
        };

        var report = new Report
        {
            ReportNumber = "HR-2026-0187",
            ReportType = ReportType.Formal,
            SubmittedUtc = DateTime.UtcNow.AddDays(-7),
            ViolationDate = DateTime.UtcNow.AddDays(-20).Date,
            ViolationDescription = "Student submitted an assignment that matched 87% with a purchased paper online.",
            IsWithinFormalFilingWindow = true,
            Student = student,
            FacultyMember = faculty,
            Course = course
        };

        var honorCase = new HonorCase
        {
            CaseNumber = "HC-2026-0187",
            Report = report,
            CurrentStatus = CaseStatus.Unassigned,
            PriorityLabel = "Second + Senior priority",
            CreatedUtc = DateTime.UtcNow.AddDays(-7),
            UpdatedUtc = DateTime.UtcNow.AddDays(-1)
        };

        honorCase.Witnesses.Add(new Witness
        {
            FullName = "Prof. Peter Parker",
            Email = "pparker@jmu.edu",
            Affiliation = "Faculty"
        });

        honorCase.StatusTimeline.Add(new CaseStatusEntry
        {
            Status = CaseStatus.ReportReceived,
            Title = "Report received",
            Notes = "Formal report submitted and queued for assignment.",
            RecordedByDisplayName = faculty.FullName,
            OccurredUtc = report.SubmittedUtc
        });

        dbContext.HonorCases.Add(honorCase);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task NormalizeSampleCourseDataAsync(CancellationToken cancellationToken)
    {
        var sampleCourses = await dbContext.Courses
            .Where(x => x.CourseCode == "CIS 484" && x.CourseName == "Web Application Development")
            .ToListAsync(cancellationToken);

        if (sampleCourses.Count == 0)
        {
            return;
        }

        foreach (var course in sampleCourses)
        {
            course.CourseName = "CIS 484";
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedApplicationUsersAsync(CancellationToken cancellationToken)
    {
        foreach (var account in _seedUserOptions.Accounts)
        {
            var existingUser = await dbContext.ApplicationUsers
                .FirstOrDefaultAsync(x => x.Email == account.Email, cancellationToken);

            if (existingUser is null)
            {
                var user = new ApplicationUser
                {
                    Email = account.Email,
                    DisplayName = account.DisplayName,
                    Role = account.Role,
                    IsActive = true
                };

                user.PasswordHash = passwordHasher.HashPassword(user, account.Password);
                dbContext.ApplicationUsers.Add(user);
            }
            else
            {
                existingUser.DisplayName = account.DisplayName;
                existingUser.Role = account.Role;
                existingUser.IsActive = true;
                existingUser.PasswordHash = passwordHasher.HashPassword(existingUser, account.Password);
                existingUser.UpdatedUtc = DateTime.UtcNow;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
