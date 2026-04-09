namespace HonorCouncil_RazorPages.Services.Interfaces;

public interface IReportIntakeService
{
    bool IsFormalReportWithinNinetyDays(DateTime violationDate, DateTime filedDateUtc);
    Task<bool> StudentHasPriorQualifyingViolationAsync(string studentNumber, CancellationToken cancellationToken = default);
}
