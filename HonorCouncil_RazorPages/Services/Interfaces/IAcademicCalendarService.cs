namespace HonorCouncil_RazorPages.Services.Interfaces;

public interface IAcademicCalendarService
{
    bool SupportsAppealDeadlineEnforcement { get; }
    bool IsWithinAppealWindow(DateTime outcomeIssuedUtc, DateTime submittedUtc);
    string GetAppealWindowMessage(DateTime? outcomeIssuedUtc = null);
}
