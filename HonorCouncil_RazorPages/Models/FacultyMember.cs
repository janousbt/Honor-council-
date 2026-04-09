using System.ComponentModel.DataAnnotations;

namespace HonorCouncil_RazorPages.Models;

public class FacultyMember
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(320)]
    public string Email { get; set; } = string.Empty;

    [StringLength(150)]
    public string? Department { get; set; }

    [StringLength(100)]
    public string? RoleTitle { get; set; }

    public ICollection<CourseFaculty> CourseAssignments { get; set; } = new List<CourseFaculty>();
    public ICollection<Report> Reports { get; set; } = new List<Report>();
    public ICollection<AvailabilitySlot> AvailabilitySlots { get; set; } = new List<AvailabilitySlot>();
}
