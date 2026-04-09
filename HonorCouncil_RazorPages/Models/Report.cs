using System.ComponentModel.DataAnnotations;
using HonorCouncil_RazorPages.Models.Enums;

namespace HonorCouncil_RazorPages.Models;

public class Report
{
    public int Id { get; set; }

    [Required, StringLength(25)]
    public string ReportNumber { get; set; } = string.Empty;

    public ReportType ReportType { get; set; }

    public DateTime SubmittedUtc { get; set; } = DateTime.UtcNow;

    [DataType(DataType.Date)]
    public DateTime ViolationDate { get; set; }

    [Required, StringLength(4000)]
    public string ViolationDescription { get; set; } = string.Empty;

    public bool IsWithinFormalFilingWindow { get; set; }
    public bool WasRedirectedToFormal { get; set; }

    [StringLength(1000)]
    public string? SubmissionNotes { get; set; }

    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public int FacultyMemberId { get; set; }
    public FacultyMember FacultyMember { get; set; } = null!;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public HonorCase? HonorCase { get; set; }
}
