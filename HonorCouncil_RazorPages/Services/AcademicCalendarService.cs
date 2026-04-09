using HonorCouncil_RazorPages.Services.Interfaces;

namespace HonorCouncil_RazorPages.Services;

public class AcademicCalendarService : IAcademicCalendarService
{
    public bool SupportsAppealDeadlineEnforcement => false;

    public bool IsWithinAppealWindow(DateTime outcomeIssuedUtc, DateTime submittedUtc)
    {
        return true;
    }

    public string GetAppealWindowMessage(DateTime? outcomeIssuedUtc = null)
    {
        return "Appeal deadline enforcement will be enabled when the academic calendar is integrated.";
    }
}
