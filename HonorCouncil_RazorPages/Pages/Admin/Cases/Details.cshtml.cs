using System.ComponentModel.DataAnnotations;
using HonorCouncil_RazorPages.Models.Enums;
using HonorCouncil_RazorPages.Services;
using HonorCouncil_RazorPages.Services.Interfaces;
using HonorCouncil_RazorPages.Services.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HonorCouncil_RazorPages.Pages.Admin.Cases;

public class DetailsModel(IAdminCaseService adminCaseService, ICurrentUserService currentUserService) : PageModel
{
    public CaseDetailViewModel CaseDetail { get; private set; } = new();
    public List<SelectListItem> InvestigatorOptions { get; private set; } = [];
    public List<SelectListItem> StatusOptions { get; private set; } = [];

    [BindProperty]
    public AssignInvestigatorInputModel AssignInput { get; set; } = new();

    [BindProperty]
    public UpdateStatusInputModel StatusInput { get; set; } = new();

    [BindProperty]
    public FinalizeOutcomeInputModel OutcomeInput { get; set; } = new();

    [BindProperty]
    public RequestClassScheduleInputModel ScheduleRequestInput { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        return await LoadPageAsync(id, cancellationToken);
    }

    public async Task<IActionResult> OnPostAssignInvestigatorAsync(CancellationToken cancellationToken)
    {
        ModelState.Clear();

        try
        {
            await adminCaseService.AssignInvestigatorAsync(AssignInput.CaseId, AssignInput.InvestigatorId, currentUserService.DisplayName, cancellationToken);
            return RedirectToPage(new { id = AssignInput.CaseId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return await LoadPageAsync(AssignInput.CaseId, cancellationToken);
        }
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(CancellationToken cancellationToken)
    {
        ModelState.Clear();
        if (!TryValidateModel(StatusInput, nameof(StatusInput)))
        {
            return await LoadPageAsync(StatusInput.CaseId, cancellationToken);
        }

        await adminCaseService.UpdateCaseStatusAsync(StatusInput.CaseId, currentUserService.DisplayName, StatusInput.Notes, StatusInput.Status, cancellationToken);
        return RedirectToPage(new { id = StatusInput.CaseId });
    }

    public async Task<IActionResult> OnPostFinalizeOutcomeAsync([FromServices] IAppealService appealService, CancellationToken cancellationToken)
    {
        ModelState.Clear();
        if (!TryValidateModel(OutcomeInput, nameof(OutcomeInput)))
        {
            return await LoadPageAsync(OutcomeInput.CaseId, cancellationToken);
        }

        await appealService.FinalizeOutcomeAsync(OutcomeInput.CaseId, OutcomeInput.OutcomeSummary, currentUserService.DisplayName, OutcomeInput.CloseCaseImmediately, cancellationToken);
        return RedirectToPage(new { id = OutcomeInput.CaseId });
    }

    public async Task<IActionResult> OnPostUploadEvidenceAsync([FromServices] ICaseEvidenceService caseEvidenceService, int id, CancellationToken cancellationToken)
    {
        var uploadFiles = (await Request.ReadFormAsync(cancellationToken)).Files.ToList();

        if (uploadFiles.Count == 0 || uploadFiles.All(x => x.Length == 0))
        {
            ModelState.Clear();
            ModelState.AddModelError(string.Empty, "Select at least one evidence file to upload.");
            return await LoadPageAsync(id, cancellationToken);
        }

        try
        {
            var uploadedIds = await caseEvidenceService.AddEvidenceAsync(id, uploadFiles, cancellationToken);
            StatusMessage = uploadedIds.Count == 1
                ? "Evidence file uploaded successfully."
                : $"{uploadedIds.Count} evidence files uploaded successfully.";
            return RedirectToPage(new { id });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.Clear();
            ModelState.AddModelError(string.Empty, ex.Message);
            return await LoadPageAsync(id, cancellationToken);
        }
    }

    public async Task<IActionResult> OnPostRequestClassScheduleAsync([FromServices] IStudentScheduleService studentScheduleService, CancellationToken cancellationToken)
    {
        try
        {
            await studentScheduleService.RequestClassScheduleAsync(ScheduleRequestInput.CaseId, currentUserService.DisplayName, cancellationToken);
            StatusMessage = "Class schedule request queued for the student.";
            return RedirectToPage(new { id = ScheduleRequestInput.CaseId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.Clear();
            ModelState.AddModelError(string.Empty, ex.Message);
            return await LoadPageAsync(ScheduleRequestInput.CaseId, cancellationToken);
        }
    }

    private async Task<IActionResult> LoadPageAsync(int id, CancellationToken cancellationToken)
    {
        var caseDetail = await adminCaseService.GetCaseDetailAsync(id, cancellationToken);
        if (caseDetail is null)
        {
            return NotFound();
        }

        CaseDetail = caseDetail;
        AssignInput.CaseId = id;
        StatusInput.CaseId = id;
        OutcomeInput.CaseId = id;
        ScheduleRequestInput.CaseId = id;
        OutcomeInput.OutcomeSummary = caseDetail.OutcomeSummary ?? string.Empty;
        StatusInput.Status = caseDetail.Status;

        InvestigatorOptions = (await adminCaseService.GetInvestigatorOptionsAsync(cancellationToken))
            .Select(x => new SelectListItem(x.DisplayName, x.Id.ToString()))
            .ToList();

        StatusOptions = Enum.GetValues<CaseStatus>()
            .Select(x => new SelectListItem(x.ToDisplayString(), x.ToString()))
            .ToList();

        return Page();
    }

    public class FinalizeOutcomeInputModel
    {
        public int CaseId { get; set; }

        [Required, Display(Name = "Outcome summary")]
        public string OutcomeSummary { get; set; } = string.Empty;

        public bool CloseCaseImmediately { get; set; }
    }

    public class AssignInvestigatorInputModel
    {
        public int CaseId { get; set; }

        [Display(Name = "Investigator")]
        public int InvestigatorId { get; set; }
    }

    public class UpdateStatusInputModel
    {
        public int CaseId { get; set; }

        [Display(Name = "Status")]
        public CaseStatus Status { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }

    public class RequestClassScheduleInputModel
    {
        public int CaseId { get; set; }
    }
}
