using System.ComponentModel.DataAnnotations;
using HonorCouncil_RazorPages.Models.Enums;

namespace HonorCouncil_RazorPages.Models;

public class Appeal
{
    public int Id { get; set; }

    public int HonorCaseId { get; set; }
    public HonorCase HonorCase { get; set; } = null!;

    public AppealStatus Status { get; set; } = AppealStatus.Submitted;

    public DateTime SubmittedUtc { get; set; } = DateTime.UtcNow;

    [Required, StringLength(200)]
    public string Grounds { get; set; } = string.Empty;

    [Required, StringLength(4000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? ReviewNotes { get; set; }

    public DateTime? ReviewedUtc { get; set; }
}
