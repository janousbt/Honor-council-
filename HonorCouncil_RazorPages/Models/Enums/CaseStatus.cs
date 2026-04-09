namespace HonorCouncil_RazorPages.Models.Enums;

public enum CaseStatus
{
    ReportReceived = 1,
    UnderReview = 2,
    Unassigned = 3,
    InvestigatorAssigned = 4,
    Investigating = 5,
    HearingRequested = 6,
    HearingScheduled = 7,
    OutcomePending = 8,
    OutcomeIssued = 9,
    AppealPending = 10,
    Closed = 11,
    NoViolation = 12
}
