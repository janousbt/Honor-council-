namespace HonorCouncil_RazorPages.Options;

public sealed class NotificationRecipientOptions
{
    public const string SectionName = "NotificationRecipients";

    public string HonorCouncilEmail { get; set; } = string.Empty;
    public string PresidentEmail { get; set; } = string.Empty;
    public string CoordinatorEmail { get; set; } = string.Empty;
}
