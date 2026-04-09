using System.ComponentModel.DataAnnotations;
using HonorCouncil_RazorPages.Models.Enums;

namespace HonorCouncil_RazorPages.Models;

public class AvailabilitySlot
{
    public int Id { get; set; }

    public int HonorCaseId { get; set; }
    public HonorCase HonorCase { get; set; } = null!;

    public int? StudentId { get; set; }
    public Student? Student { get; set; }

    public int? FacultyMemberId { get; set; }
    public FacultyMember? FacultyMember { get; set; }

    public AvailabilityParticipantRole ParticipantRole { get; set; }

    [Required, StringLength(200)]
    public string SubmittedByName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(320)]
    public string SubmittedByEmail { get; set; } = string.Empty;

    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
}
