using System.ComponentModel.DataAnnotations;

namespace HonorCouncil_RazorPages.Models;

public class StudentScheduleFile
{
    public int Id { get; set; }

    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    [Required, StringLength(255)]
    public string OriginalFileName { get; set; } = string.Empty;

    [Required, StringLength(255)]
    public string StoredFileName { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string ContentType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public DateTime UploadedUtc { get; set; } = DateTime.UtcNow;

    [Required, StringLength(200)]
    public string UploadedByDisplayName { get; set; } = string.Empty;
}
