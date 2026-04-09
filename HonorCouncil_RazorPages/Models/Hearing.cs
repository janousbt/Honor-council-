using System.ComponentModel.DataAnnotations;
using HonorCouncil_RazorPages.Models.Enums;

namespace HonorCouncil_RazorPages.Models;

public class Hearing
{
    public int Id { get; set; }

    public int HonorCaseId { get; set; }
    public HonorCase HonorCase { get; set; } = null!;

    public DateTime? ScheduledStartUtc { get; set; }

    public HearingFormat HearingFormat { get; set; } = HearingFormat.InPerson;

    [StringLength(250)]
    public string? LocationOrMeetingLink { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}
