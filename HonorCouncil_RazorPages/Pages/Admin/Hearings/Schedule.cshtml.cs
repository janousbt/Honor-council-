using System.ComponentModel.DataAnnotations;
using HonorCouncil_RazorPages.Models.Enums;
using HonorCouncil_RazorPages.Services.Interfaces;
using HonorCouncil_RazorPages.Services.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HonorCouncil_RazorPages.Pages.Admin.Hearings;

public class ScheduleModel(IHearingService hearingService, ICurrentUserService currentUserService) : PageModel
{
    public HearingScheduleViewModel Details { get; private set; } = new();
    public List<SelectListItem> FormatOptions { get; private set; } = [];

    [BindProperty]
    public ScheduleInputModel Input { get; set; } = new();

    [BindProperty]
    public RequestAvailabilityInputModel RequestInput { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int caseId, CancellationToken cancellationToken)
    {
        return await LoadAsync(caseId, cancellationToken);
    }

    public async Task<IActionResult> OnPostRequestAvailabilityAsync(CancellationToken cancellationToken)
    {
        await hearingService.RequestAvailabilityAsync(RequestInput.CaseId, currentUserService.DisplayName, cancellationToken);
        return RedirectToPage(new { caseId = RequestInput.CaseId });
    }

    public async Task<IActionResult> OnPostScheduleAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return await LoadAsync(Input.CaseId, cancellationToken);
        }

        await hearingService.ScheduleHearingAsync(new HearingScheduleInput
        {
            CaseId = Input.CaseId,
            ScheduledLocal = Input.ScheduledLocal,
            HearingFormat = Input.HearingFormat,
            LocationOrMeetingLink = Input.LocationOrMeetingLink,
            Notes = Input.Notes
        }, currentUserService.DisplayName, cancellationToken);

        return RedirectToPage(new { caseId = Input.CaseId });
    }

    private async Task<IActionResult> LoadAsync(int caseId, CancellationToken cancellationToken)
    {
        var details = await hearingService.GetScheduleDetailsAsync(caseId, cancellationToken);
        if (details is null)
        {
            return NotFound();
        }

        Details = details;
        Input.CaseId = caseId;
        Input.ScheduledLocal = RoundToFiveMinutes(details.ScheduledStartUtc?.ToLocalTime() ?? DateTime.Now.AddDays(3));
        Input.HearingFormat = details.HearingFormat;
        Input.LocationOrMeetingLink = details.LocationOrMeetingLink;
        Input.Notes = details.Notes;
        RequestInput.CaseId = caseId;
        FormatOptions = hearingService.GetSupportedFormats()
            .Select(x => new SelectListItem(
                x == HearingFormat.InPerson ? "In Person" : x.ToString(),
                x.ToString()))
            .ToList();

        return Page();
    }

    private static DateTime RoundToFiveMinutes(DateTime value)
    {
        var roundedMinutes = (int)Math.Ceiling(value.Minute / 5d) * 5;
        var rounded = new DateTime(value.Year, value.Month, value.Day, value.Hour, 0, 0, value.Kind).AddMinutes(roundedMinutes);
        return rounded;
    }

    public class ScheduleInputModel
    {
        public int CaseId { get; set; }

        [Required, Display(Name = "Scheduled date/time")]
        public DateTime ScheduledLocal { get; set; }

        [Display(Name = "Hearing format")]
        public HearingFormat HearingFormat { get; set; }

        [Display(Name = "Location or meeting link")]
        public string? LocationOrMeetingLink { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }

    public class RequestAvailabilityInputModel
    {
        public int CaseId { get; set; }
    }
}
