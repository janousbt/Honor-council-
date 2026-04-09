using HonorCouncil_RazorPages.Models.Enums;
using HonorCouncil_RazorPages.Services.Interfaces;
using HonorCouncil_RazorPages.Services.Models;
using HonorCouncil_RazorPages.Models;

namespace HonorCouncil_RazorPages.Services;

public class CaseWorkflowService : ICaseWorkflowService
{
    private static readonly IReadOnlyList<CaseStatus> Timeline =
    [
        CaseStatus.ReportReceived,
        CaseStatus.InvestigatorAssigned,
        CaseStatus.Investigating,
        CaseStatus.HearingScheduled,
        CaseStatus.OutcomeIssued,
        CaseStatus.AppealPending,
        CaseStatus.Closed
    ];

    private static readonly IReadOnlyList<TimelineStepDefinition> FormalStudentTimeline =
    [
        new("Report received", [CaseStatus.ReportReceived]),
        new("Case under review", [CaseStatus.UnderReview, CaseStatus.Unassigned]),
        new("Investigator assigned", [CaseStatus.InvestigatorAssigned]),
        new("Investigation in progress", [CaseStatus.Investigating]),
        new("Hearing coordination", [CaseStatus.HearingRequested, CaseStatus.HearingScheduled]),
        new("Outcome issued", [CaseStatus.OutcomePending, CaseStatus.OutcomeIssued]),
        new("Appeal review", [CaseStatus.AppealPending]),
        new("Case closed", [CaseStatus.Closed, CaseStatus.NoViolation])
    ];

    private static readonly IReadOnlyList<TimelineStepDefinition> InformalStudentTimeline =
    [
        new("Report received", [CaseStatus.ReportReceived]),
        new("Informal review", [CaseStatus.UnderReview]),
        new("Resolution issued", [CaseStatus.OutcomePending, CaseStatus.OutcomeIssued]),
        new("Case closed", [CaseStatus.Closed, CaseStatus.NoViolation])
    ];

    public IReadOnlyList<CaseStatus> GetOrderedTimeline() => Timeline;

    public IReadOnlyList<StudentTimelineStepViewModel> GetStudentTimeline(
        ReportType reportType,
        CaseStatus currentStatus,
        IReadOnlyList<CaseStatusEntry> statusEntries)
    {
        var definitions = reportType == ReportType.Formal ? FormalStudentTimeline : InformalStudentTimeline;
        var currentIndex = FindCurrentIndex(definitions, currentStatus);

        if (currentIndex < 0)
        {
            currentIndex = 0;
        }

        var orderedEntries = statusEntries
            .OrderBy(entry => entry.OccurredUtc)
            .ToList();

        return definitions
            .Take(currentIndex + 1)
            .Select((step, index) =>
            {
                var matchingEntry = orderedEntries.LastOrDefault(entry => step.Statuses.Contains(entry.Status));
                return new StudentTimelineStepViewModel
                {
                    Title = step.Title,
                    Detail = matchingEntry is null ? null : $"{matchingEntry.Title} - {matchingEntry.OccurredUtc:MMMM d}",
                    State = index == currentIndex ? "current" : "done"
                };
            })
            .ToList();
    }

    private static int FindCurrentIndex(IReadOnlyList<TimelineStepDefinition> definitions, CaseStatus currentStatus)
    {
        for (var i = 0; i < definitions.Count; i++)
        {
            if (definitions[i].Statuses.Contains(currentStatus))
            {
                return i;
            }
        }

        return -1;
    }

    private sealed record TimelineStepDefinition(string Title, IReadOnlyList<CaseStatus> Statuses);
}
