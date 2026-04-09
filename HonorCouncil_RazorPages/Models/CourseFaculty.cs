namespace HonorCouncil_RazorPages.Models;

public class CourseFaculty
{
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public int FacultyMemberId { get; set; }
    public FacultyMember FacultyMember { get; set; } = null!;
}
