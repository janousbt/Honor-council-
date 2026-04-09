using System.ComponentModel.DataAnnotations;
using HonorCouncil_RazorPages.Services.Interfaces;
using HonorCouncil_RazorPages.Services.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HonorCouncil_RazorPages.Pages.Reports.Informal;

public class CreateModel(IReportIntakeService reportIntakeService, IReportSubmissionService reportSubmissionService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool IsWithinWindow { get; private set; }

    public void OnGet()
    {
        SeedDefaults();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
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
            var result = await reportSubmissionService.SubmitInformalReportAsync(Input.ToSubmissionInput(), cancellationToken);
            return RedirectToPage("/Reports/Confirmation", new { caseNumber = result.CaseNumber });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ReportAlert"] = ex.Message;
            return RedirectToPage("/Reports/Formal/Create");
        }
    }

    private void SeedDefaults()
    {
        Input.ReportDate = DateTime.Today;
        Input.ViolationDate = DateTime.Today;
        IsWithinWindow = true;
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

        [Required, EmailAddress, Display(Name = "Student email")]
        public string StudentEmail { get; set; } = string.Empty;

        [Display(Name = "Academic year")]
        public string? StudentAcademicYear { get; set; }

        [Required, Display(Name = "Date of violation"), DataType(DataType.Date)]
        public DateTime ViolationDate { get; set; }

        [DataType(DataType.Date), Display(Name = "Date of agreement")]
        public DateTime? AgreementDate { get; set; }

        [Required, StringLength(4000), Display(Name = "Violation description")]
        public string Description { get; set; } = string.Empty;

        [Required, StringLength(1000), Display(Name = "Resolution summary")]
        public string SubmissionNotes { get; set; } = string.Empty;

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
                StudentEmail = StudentEmail,
                StudentAcademicYear = StudentAcademicYear,
                ViolationDate = ViolationDate,
                Description = Description,
                SubmissionNotes = SubmissionNotes,
                Witnesses = new List<WitnessInput>()
            };
        }
    }
}
