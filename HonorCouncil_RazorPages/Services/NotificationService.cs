using HonorCouncil_RazorPages.Data;
using HonorCouncil_RazorPages.Models;
using HonorCouncil_RazorPages.Services.Interfaces;

namespace HonorCouncil_RazorPages.Services;

public class NotificationService(HonorCouncilDbContext dbContext) : INotificationService
{
    public async Task<NotificationLog> QueueNotificationAsync(
        string recipientEmail,
        string subject,
        string notificationType,
        int? honorCaseId = null,
        int? reportId = null,
        CancellationToken cancellationToken = default)
    {
        var notification = new NotificationLog
        {
            RecipientEmail = recipientEmail,
            Subject = subject,
            NotificationType = notificationType,
            HonorCaseId = honorCaseId,
            ReportId = reportId,
            WasSuccessful = false,
            Notes = "Queued only. SMTP integration will be added in a later phase."
        };

        dbContext.NotificationLogs.Add(notification);
        await dbContext.SaveChangesAsync(cancellationToken);
        return notification;
    }
}
