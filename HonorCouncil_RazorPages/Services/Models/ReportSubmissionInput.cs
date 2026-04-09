namespace HonorCouncil_RazorPages.Services.Models;

public class ReportSubmissionInput
{
    public string FacultyName { get; set; } = string.Empty;
    public string FacultyEmail { get; set; } = string.Empty;
    public string? FacultyDepartment { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string? CourseSection { get; set; }
    public string? Term { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public string? StudentMajor { get; set; }
    public string? StudentAcademicYear { get; set; }
    public DateTime ViolationDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? SubmissionNotes { get; set; }
    public bool WasRedirectedToFormal { get; set; }
    public List<WitnessInput> Witnesses { get; set; } = [];
}

public class WitnessInput
{
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Affiliation { get; set; }
}
