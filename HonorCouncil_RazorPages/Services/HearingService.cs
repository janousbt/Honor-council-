using HonorCouncil_RazorPages.Data;
using HonorCouncil_RazorPages.Models;
using HonorCouncil_RazorPages.Models.Enums;
using HonorCouncil_RazorPages.Services.Interfaces;
using HonorCouncil_RazorPages.Services.Models;
using Microsoft.EntityFrameworkCore;

namespace HonorCouncil_RazorPages.Services;

public class HearingService(HonorCouncilDbContext dbContext, INotificationService notificationService) : IHearingService
{
    private static readonly IReadOnlyList<HearingFormat> Formats =
    [
        HearingFormat.InPerson,
        HearingFormat.Online
    ];

    public IReadOnlyList<HearingFormat> GetSupportedFormats() => Formats;

    public async Task<HearingQueueViewModel> GetHearingQueueAsync(CancellationToken cancellationToken = default)
    {
        var query = dbContext.HonorCases
            .AsNoTracking()
            .Include(x => x.Report).ThenInclude(x => x.Student)
            .Include(x => x.Report).ThenInclude(x => x.Course)
            .Include(x => x.AssignedInvestigator)
            .Include(x => x.Hearing)
            .Where(x => x.Report.ReportType == ReportType.Formal);

        var needsScheduling = await query
            .Where(x => x.Hearing == null || x.Hearing.ScheduledStartUtc == null)
            .OrderBy(x => x.CurrentStatus)
            .ThenBy(x => x.Report.SubmittedUtc)
            .Select(MapQueueItem())
            .ToListAsync(cancellationToken);

        var scheduled = await query
            .Where(x => x.Hearing != null && x.Hearing.ScheduledStartUtc != null)
            .OrderBy(x => x.Hearing!.ScheduledStartUtc)
            .Select(MapQueueItem())
            .ToListAsync(cancellationToken);

        return new HearingQueueViewModel
        {
            NeedScheduling = needsScheduling,
            ScheduledHearings = scheduled
        };
    }

    public async Task<HearingScheduleViewModel?> GetScheduleDetailsAsync(int caseId, CancellationToken cancellationToken = default)
    {
        return await dbContext.HonorCases
            .AsNoTracking()
            .Include(x => x.Report).ThenInclude(x => x.Student)
            .Include(x => x.Report).ThenInclude(x => x.Student).ThenInclude(x => x.ScheduleFiles)
            .Include(x => x.Report).ThenInclude(x => x.FacultyMember)
            .Include(x => x.Report).ThenInclude(x => x.Course)
            .Include(x => x.AssignedInvestigator)
            .Include(x => x.Hearing)
            .Include(x => x.AvailabilitySlots)
            .Where(x => x.Id == caseId)
            .Select(x => new HearingScheduleViewModel
            {
                CaseId = x.Id,
                CaseNumber = x.CaseNumber,
                StudentName = x.Report.Student.FullName,
                StudentEmail = x.Report.Student.Email,
                FacultyName = x.Report.FacultyMember.FullName,
                FacultyEmail = x.Report.FacultyMember.Email,
                CourseDisplay = $"{x.Report.Course.CourseCode} - {x.Report.Course.CourseName}",
                InvestigatorName = x.AssignedInvestigator != null ? x.AssignedInvestigator.FullName : null,
                InvestigatorEmail = x.AssignedInvestigator != null ? x.AssignedInvestigator.Email : null,
                ScheduledStartUtc = x.Hearing != null ? x.Hearing.ScheduledStartUtc : null,
                HearingFormat = x.Hearing != null ? x.Hearing.HearingFormat : HearingFormat.InPerson,
                LocationOrMeetingLink = x.Hearing != null ? x.Hearing.LocationOrMeetingLink : null,
                Notes = x.Hearing != null ? x.Hearing.Notes : null,
                StudentScheduleFiles = x.Report.Student.ScheduleFiles
                    .OrderByDescending(s => s.UploadedUtc)
                    .Select(s => new StudentScheduleItemViewModel
                    {
                        Id = s.Id,
                        FileName = s.OriginalFileName,
                        UploadedBy = s.UploadedByDisplayName,
                        UploadedUtcDisplay = s.UploadedUtc.ToLocalTime().ToString("MMMM d, yyyy h:mm tt")
                    }).ToList(),
                AvailabilitySlots = x.AvailabilitySlots
                    .OrderBy(a => a.StartUtc)
                    .Select(a => new AvailabilitySlotViewModel
                    {
                        SubmittedByName = a.SubmittedByName,
                        SubmittedByEmail = a.SubmittedByEmail,
                        ParticipantRole = a.ParticipantRole,
                        StartUtc = a.StartUtc,
                        EndUtc = a.EndUtc
                    }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task ScheduleHearingAsync(HearingScheduleInput input, string performedBy, CancellationToken cancellationToken = default)
    {
        var honorCase = await dbContext.HonorCases
            .Include(x => x.Hearing)
            .Include(x => x.Report).ThenInclude(x => x.Student)
            .Include(x => x.Report).ThenInclude(x => x.FacultyMember)
            .Include(x => x.StatusTimeline)
            .FirstOrDefaultAsync(x => x.Id == input.CaseId, cancellationToken)
            ?? throw new InvalidOperationException("Case not found.");

        if (honorCase.Hearing is null)
        {
            honorCase.Hearing = new Hearing { HonorCaseId = honorCase.Id };
        }

        honorCase.Hearing.ScheduledStartUtc = DateTime.SpecifyKind(input.ScheduledLocal, DateTimeKind.Local).ToUniversalTime();
        honorCase.Hearing.HearingFormat = input.HearingFormat;
        honorCase.Hearing.LocationOrMeetingLink = input.LocationOrMeetingLink;
        honorCase.Hearing.Notes = input.Notes;
        honorCase.CurrentStatus = CaseStatus.HearingScheduled;
        honorCase.UpdatedUtc = DateTime.UtcNow;
        honorCase.StatusTimeline.Add(new CaseStatusEntry
        {
            Status = CaseStatus.HearingScheduled,
            Title = "Hearing scheduled",
            Notes = $"{input.HearingFormat} hearing scheduled for {input.ScheduledLocal:f}.",
            RecordedByDisplayName = performedBy,
            OccurredUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        await notificationService.QueueNotificationAsync(honorCase.Report.Student.Email, $"Hearing scheduled: {honorCase.CaseNumber}", "HearingScheduled", honorCase.Id, honorCase.ReportId, cancellationToken);
        await notificationService.QueueNotificationAsync(honorCase.Report.FacultyMember.Email, $"Hearing scheduled: {honorCase.CaseNumber}", "HearingScheduled", honorCase.Id, honorCase.ReportId, cancellationToken);
    }

    public async Task RequestAvailabilityAsync(int caseId, string performedBy, CancellationToken cancellationToken = default)
    {
        var honorCase = await dbContext.HonorCases
            .Include(x => x.Report).ThenInclude(x => x.Student)
            .Include(x => x.Report).ThenInclude(x => x.FacultyMember)
            .Include(x => x.AssignedInvestigator)
            .Include(x => x.StatusTimeline)
            .FirstOrDefaultAsync(x => x.Id == caseId, cancellationToken)
            ?? throw new InvalidOperationException("Case not found.");

        var requestedRoles = new List<string> { "student", "faculty" };
        if (honorCase.AssignedInvestigator is not null && !string.IsNullOrWhiteSpace(honorCase.AssignedInvestigator.Email))
        {
            requestedRoles.Add("investigator");
        }

        honorCase.CurrentStatus = CaseStatus.HearingRequested;
        honorCase.UpdatedUtc = DateTime.UtcNow;
        honorCase.StatusTimeline.Add(new CaseStatusEntry
        {
            Status = CaseStatus.HearingRequested,
            Title = "Availability requested",
            Notes = $"Availability requested from {string.Join(", ", requestedRoles)}.",
            RecordedByDisplayName = performedBy,
            OccurredUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        await notificationService.QueueNotificationAsync(honorCase.Report.Student.Email, $"Availability request: {honorCase.CaseNumber}", "AvailabilityRequest", honorCase.Id, honorCase.ReportId, cancellationToken);
        await notificationService.QueueNotificationAsync(honorCase.Report.FacultyMember.Email, $"Availability request: {honorCase.CaseNumber}", "AvailabilityRequest", honorCase.Id, honorCase.ReportId, cancellationToken);
        if (honorCase.AssignedInvestigator is not null && !string.IsNullOrWhiteSpace(honorCase.AssignedInvestigator.Email))
        {
            await notificationService.QueueNotificationAsync(honorCase.AssignedInvestigator.Email, $"Availability request: {honorCase.CaseNumber}", "AvailabilityRequest", honorCase.Id, honorCase.ReportId, cancellationToken);
        }
    }

    public async Task SubmitAvailabilityAsync(AvailabilitySubmissionInput input, CancellationToken cancellationToken = default)
    {
        if (input.EndLocal <= input.StartLocal)
        {
            throw new InvalidOperationException("Availability end time must be after the start time.");
        }

        var honorCase = await dbContext.HonorCases
            .Include(x => x.Report).ThenInclude(x => x.Student)
            .Include(x => x.Report).ThenInclude(x => x.FacultyMember)
            .Include(x => x.StatusTimeline)
            .FirstOrDefaultAsync(x => x.Id == input.CaseId, cancellationToken)
            ?? throw new InvalidOperationException("Case not found.");

        var submittedEmail = input.SubmittedByEmail.Trim();
        int? studentId = null;
        int? facultyMemberId = null;

        switch (input.ParticipantRole)
        {
            case AvailabilityParticipantRole.Student:
                if (!string.Equals(honorCase.Report.Student.Email, submittedEmail, StringComparison.OrdinalIgnoreCase))
                {
                    throw new UnauthorizedAccessException("You do not have access to submit availability for this case.");
                }

                studentId = honorCase.Report.StudentId;
                break;

            case AvailabilityParticipantRole.Faculty:
                if (!string.Equals(honorCase.Report.FacultyMember.Email, submittedEmail, StringComparison.OrdinalIgnoreCase))
                {
                    throw new UnauthorizedAccessException("You do not have access to submit availability for this case.");
                }

                facultyMemberId = honorCase.Report.FacultyMemberId;
                break;
        }

        var availability = new AvailabilitySlot
        {
            HonorCaseId = input.CaseId,
            StudentId = studentId,
            FacultyMemberId = facultyMemberId,
            SubmittedByName = input.SubmittedByName,
            SubmittedByEmail = submittedEmail,
            ParticipantRole = input.ParticipantRole,
            StartUtc = DateTime.SpecifyKind(input.StartLocal, DateTimeKind.Local).ToUniversalTime(),
            EndUtc = DateTime.SpecifyKind(input.EndLocal, DateTimeKind.Local).ToUniversalTime()
        };

        dbContext.AvailabilitySlots.Add(availability);
        honorCase.StatusTimeline.Add(new CaseStatusEntry
        {
            Status = honorCase.CurrentStatus,
            Title = "Availability submitted",
            Notes = $"{input.ParticipantRole} availability submitted by {input.SubmittedByName}.",
            RecordedByDisplayName = input.SubmittedByName,
            OccurredUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ParticipantCaseOptionViewModel>> GetStudentCasesAsync(string studentEmail, CancellationToken cancellationToken = default)
    {
        return await dbContext.HonorCases
            .AsNoTracking()
            .Where(x => x.Report.Student.Email == studentEmail && x.Report.ReportType == ReportType.Formal)
            .OrderByDescending(x => x.Report.SubmittedUtc)
            .Select(x => new ParticipantCaseOptionViewModel
            {
                CaseId = x.Id,
                CaseNumber = x.CaseNumber,
                Label = $"{x.CaseNumber} - {x.Report.Course.CourseCode}"
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ParticipantCaseOptionViewModel>> GetFacultyCasesAsync(string facultyEmail, CancellationToken cancellationToken = default)
    {
        return await dbContext.HonorCases
            .AsNoTracking()
            .Where(x => x.Report.FacultyMember.Email == facultyEmail && x.Report.ReportType == ReportType.Formal)
            .OrderByDescending(x => x.Report.SubmittedUtc)
            .Select(x => new ParticipantCaseOptionViewModel
            {
                CaseId = x.Id,
                CaseNumber = x.CaseNumber,
                Label = $"{x.CaseNumber} - {x.Report.Student.FullName}"
            })
            .ToListAsync(cancellationToken);
    }

    private static System.Linq.Expressions.Expression<Func<HonorCase, HearingQueueItemViewModel>> MapQueueItem()
    {
        return x => new HearingQueueItemViewModel
        {
            CaseId = x.Id,
            CaseNumber = x.CaseNumber,
            StudentName = x.Report.Student.FullName,
            CourseDisplay = x.Report.Course.CourseCode,
            StatusDisplay = x.CurrentStatus.ToDisplayString(),
            InvestigatorName = x.AssignedInvestigator != null ? x.AssignedInvestigator.FullName : null,
            ScheduledStartUtc = x.Hearing != null ? x.Hearing.ScheduledStartUtc : null,
            HearingFormat = x.Hearing != null ? x.Hearing.HearingFormat : null
        };
    }
}
