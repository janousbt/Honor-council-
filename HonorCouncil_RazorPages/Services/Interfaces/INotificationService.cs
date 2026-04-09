using HonorCouncil_RazorPages.Models;

namespace HonorCouncil_RazorPages.Services.Interfaces;

public interface INotificationService
{
    Task<NotificationLog> QueueNotificationAsync(string recipientEmail, string subject, string notificationType, int? honorCaseId = null, int? reportId = null, CancellationToken cancellationToken = default);
}
