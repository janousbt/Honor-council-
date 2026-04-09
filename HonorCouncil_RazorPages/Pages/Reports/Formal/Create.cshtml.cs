using System.ComponentModel.DataAnnotations;
using HonorCouncil_RazorPages.Services.Interfaces;
using HonorCouncil_RazorPages.Services.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HonorCouncil_RazorPages.Pages.Reports.Formal;

public class CreateModel(IReportIntakeService reportIntakeService, IReportSubmissionService reportSubmissionService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty]
    public List<IFormFile> EvidenceFiles { get; set; } = [];

    [TempData]
    public string? AlertMessage { get; set; }

    public bool IsWithinWindow { get; private set; }

    public void OnGet()
    {
        SeedDefaults();

        if (TempData.TryGetValue("ReportAlert", out var alert))
        {
            AlertMessage = alert?.ToString();
        }
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        EnsureWitnessSlots();

        if (Input.ReportDate == default)
        {
            Input.ReportDate = DateTime.Today;
        }

        IsWithinWindow = reportIntakeService.IsFormalReportWithinNinetyDays(Input.ViolationDate, DateTime.UtcNow);
        ValidateViolationDateWindow();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var result = await reportSubmissionService.SubmitFormalReportAsync(
                Input.ToSubmissionInput(),
                EvidenceFiles,
                cancellationToken);

            return RedirectToPage("/Reports/Confirmation", new { caseNumber = result.CaseNumber });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }

    private void SeedDefaults()
    {
        Input.ReportDate = DateTime.Today;
        Input.ViolationDate = DateTime.Today;

        if (Input.PossibleViolationDate == null)
        {
            Input.PossibleViolationDate = DateTime.Today;
        }

        EnsureWitnessSlots();
        IsWithinWindow = true;
    }

    private void EnsureWitnessSlots()
    {
        while (Input.Witnesses.Count < 2)
        {
            Input.Witnesses.Add(new WitnessInputModel());
        }
    }

    private void ValidateViolationDateWindow()
    {
        if (ModelState.ContainsKey($"{nameof(Input)}.{nameof(Input.ViolationDate)}") &&
            ModelState[$"{nameof(Input)}.{nameof(Input.ViolationDate)}"]!.Errors.Count > 0)
        {
            return;
        }

        if (IsWithinWindow)
        {
            return;
        }

        const string message = "Reports must be filed within 90 days of the violation date.";
        ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.ViolationDate)}", message);
        ModelState.AddModelError(string.Empty, message);
    }

    public class InputModel
    {
        [Required, Display(Name = "Faculty name")]
        public string FacultyName { get; set; } = string.Empty;

        [Required, EmailAddress, Display(Name = "Faculty email")]
        public string FacultyEmail { get; set; } = string.Empty;

        [Display(Name = "Office")]
        public string? FacultyDepartment { get; set; }

        [Phone, Display(Name = "Faculty phone")]
        public string? FacultyPhone { get; set; }

        [Display(Name = "Report date"), DataType(DataType.Date)]
        public DateTime ReportDate { get; set; }

        [Required, Display(Name = "Course number")]
        public string CourseCode { get; set; } = string.Empty;

        [Required, Display(Name = "Course name")]
        public string CourseName { get; set; } = string.Empty;

        [Display(Name = "Section")]
        public string? CourseSection { get; set; }

        [Display(Name = "Term")]
        public string? Term { get; set; }

        [Display(Name = "Course format")]
        public string? CourseFormat { get; set; }

        [Required, Display(Name = "Student name")]
        public string StudentName { get; set; } = string.Empty;

        [Required, Display(Name = "Student ID")]
        public string StudentNumber { get; set; } = string.Empty;

        [EmailAddress, Display(Name = "Student email")]
        public string? StudentEmail { get; set; }

        [Phone, Display(Name = "Student phone")]
        public string? StudentPhone { get; set; }

        [Display(Name = "Student major")]
        public string? StudentMajor { get; set; }

        [Display(Name = "Academic year")]
        public string? StudentAcademicYear { get; set; }

        [Display(Name = "Date of possible violation"), DataType(DataType.Date)]
        public DateTime? PossibleViolationDate { get; set; }

        [Required, Display(Name = "Date of violation"), DataType(DataType.Date)]
        public DateTime ViolationDate { get; set; }

        [Required, StringLength(4000), Display(Name = "Violation description")]
        public string Description { get; set; } = string.Empty;

        [StringLength(1000), Display(Name = "Additional notes")]
        public string? SubmissionNotes { get; set; }

        public List<WitnessInputModel> Witnesses { get; set; } = [];

        public ReportSubmissionInput ToSubmissionInput()
        {
            return new ReportSubmissionInput
            {
                FacultyName = FacultyName,
                FacultyEmail = FacultyEmail,
                FacultyDepartment = FacultyDepartment,
                CourseCode = CourseCode,
                CourseName = CourseName,
                CourseSection = CourseSection,
                Term = Term,
                StudentName = StudentName,
                StudentNumber = StudentNumber,
                StudentEmail = StudentEmail ?? string.Empty,
                StudentMajor = StudentMajor,
                StudentAcademicYear = StudentAcademicYear,
                ViolationDate = ViolationDate,
                Description = Description,
                SubmissionNotes = SubmissionNotes,
                Witnesses = Witnesses
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x.FullName) ||
                        !string.IsNullOrWhiteSpace(x.Email) ||
                        !string.IsNullOrWhiteSpace(x.Affiliation))
                    .Select(x => x.ToWitnessInput())
                    .ToList()
            };
        }
    }

    public class WitnessInputModel
    {
        public string? FullName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? Affiliation { get; set; }

        public WitnessInput ToWitnessInput()
        {
            return new WitnessInput
            {
                FullName = FullName ?? string.Empty,
                Email = Email,
                Affiliation = Affiliation
            };
        }
    }
}
