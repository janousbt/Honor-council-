using System.ComponentModel.DataAnnotations;

namespace HonorCouncil_RazorPages.Models;

public class Course
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string CourseCode { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string CourseName { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Section { get; set; }

    [StringLength(50)]
    public string? Term { get; set; }

    public ICollection<CourseFaculty> CourseAssignments { get; set; } = new List<CourseFaculty>();
    public ICollection<Report> Reports { get; set; } = new List<Report>();
}
