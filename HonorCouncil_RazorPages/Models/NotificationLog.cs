using System.ComponentModel.DataAnnotations;

namespace HonorCouncil_RazorPages.Models;

public class NotificationLog
{
    public int Id { get; set; }

    public int? HonorCaseId { get; set; }
    public HonorCase? HonorCase { get; set; }

    public int? ReportId { get; set; }
    public Report? Report { get; set; }

    [Required, EmailAddress, StringLength(320)]
    public string RecipientEmail { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string NotificationType { get; set; } = string.Empty;

    public DateTime SentUtc { get; set; } = DateTime.UtcNow;

    public bool WasSuccessful { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}
