using HonorCouncil_RazorPages.Models.Enums;

namespace HonorCouncil_RazorPages.Services.Models;

public class HearingQueueViewModel
{
    public IReadOnlyList<HearingQueueItemViewModel> NeedScheduling { get; set; } = [];
    public IReadOnlyList<HearingQueueItemViewModel> ScheduledHearings { get; set; } = [];
}

public class HearingQueueItemViewModel
{
    public int CaseId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string CourseDisplay { get; set; } = string.Empty;
    public string StatusDisplay { get; set; } = string.Empty;
    public string? InvestigatorName { get; set; }
    public DateTime? ScheduledStartUtc { get; set; }
    public HearingFormat? HearingFormat { get; set; }
}

public class HearingScheduleViewModel
{
    public int CaseId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public string FacultyName { get; set; } = string.Empty;
    public string FacultyEmail { get; set; } = string.Empty;
    public string CourseDisplay { get; set; } = string.Empty;
    public string? InvestigatorName { get; set; }
    public string? InvestigatorEmail { get; set; }
    public IReadOnlyList<AvailabilitySlotViewModel> AvailabilitySlots { get; set; } = [];
    public IReadOnlyList<StudentScheduleItemViewModel> StudentScheduleFiles { get; set; } = [];
    public DateTime? ScheduledStartUtc { get; set; }
    public HearingFormat HearingFormat { get; set; }
    public string? LocationOrMeetingLink { get; set; }
    public string? Notes { get; set; }
}

public class AvailabilitySlotViewModel
{
    public string SubmittedByName { get; set; } = string.Empty;
    public string SubmittedByEmail { get; set; } = string.Empty;
    public AvailabilityParticipantRole ParticipantRole { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
}

public class HearingScheduleInput
{
    public int CaseId { get; set; }
    public DateTime ScheduledLocal { get; set; }
    public HearingFormat HearingFormat { get; set; }
    public string? LocationOrMeetingLink { get; set; }
    public string? Notes { get; set; }
}

public class AvailabilitySubmissionInput
{
    public int CaseId { get; set; }
    public string SubmittedByName { get; set; } = string.Empty;
    public string SubmittedByEmail { get; set; } = string.Empty;
    public AvailabilityParticipantRole ParticipantRole { get; set; }
    public int? StudentId { get; set; }
    public int? FacultyMemberId { get; set; }
    public DateTime StartLocal { get; set; }
    public DateTime EndLocal { get; set; }
}

public class ParticipantCaseOptionViewModel
{
    public int CaseId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}
