using System.ComponentModel.DataAnnotations;
using HonorCouncil_RazorPages.Models.Enums;

namespace HonorCouncil_RazorPages.Models;

public class CaseStatusEntry
{
    public int Id { get; set; }

    public int HonorCaseId { get; set; }
    public HonorCase HonorCase { get; set; } = null!;

    public CaseStatus Status { get; set; }

    [Required, StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Notes { get; set; }

    public DateTime OccurredUtc { get; set; } = DateTime.UtcNow;

    [StringLength(200)]
    public string RecordedByDisplayName { get; set; } = string.Empty;
}
