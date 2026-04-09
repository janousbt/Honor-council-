using System.ComponentModel.DataAnnotations;
using HonorCouncil_RazorPages.Models.Enums;

namespace HonorCouncil_RazorPages.Models;

public class HonorCase
{
    public int Id { get; set; }

    [Required, StringLength(25)]
    public string CaseNumber { get; set; } = string.Empty;

    public int ReportId { get; set; }
    public Report Report { get; set; } = null!;

    public CaseStatus CurrentStatus { get; set; } = CaseStatus.ReportReceived;

    [StringLength(100)]
    public string? PriorityLabel { get; set; }

    [StringLength(2000)]
    public string? OutcomeSummary { get; set; }

    public DateTime? OutcomeIssuedUtc { get; set; }

    public int? AssignedInvestigatorId { get; set; }
    public Investigator? AssignedInvestigator { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;

    public Hearing? Hearing { get; set; }
    public Appeal? Appeal { get; set; }
    public ICollection<EvidenceFile> EvidenceFiles { get; set; } = new List<EvidenceFile>();
    public ICollection<Witness> Witnesses { get; set; } = new List<Witness>();
    public ICollection<CaseStatusEntry> StatusTimeline { get; set; } = new List<CaseStatusEntry>();
    public ICollection<AvailabilitySlot> AvailabilitySlots { get; set; } = new List<AvailabilitySlot>();
    public ICollection<NotificationLog> Notifications { get; set; } = new List<NotificationLog>();
}
