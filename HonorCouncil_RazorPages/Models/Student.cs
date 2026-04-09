using System.ComponentModel.DataAnnotations;

namespace HonorCouncil_RazorPages.Models;

public class Student
{
    public int Id { get; set; }

    [Required, StringLength(32)]
    public string StudentNumber { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(320)]
    public string Email { get; set; } = string.Empty;

    [StringLength(150)]
    public string? Major { get; set; }

    [StringLength(50)]
    public string? AcademicYear { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Report> Reports { get; set; } = new List<Report>();
    public ICollection<AvailabilitySlot> AvailabilitySlots { get; set; } = new List<AvailabilitySlot>();
    public ICollection<StudentScheduleFile> ScheduleFiles { get; set; } = new List<StudentScheduleFile>();
}
