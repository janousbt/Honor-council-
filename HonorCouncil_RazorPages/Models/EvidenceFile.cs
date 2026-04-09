using System.ComponentModel.DataAnnotations;

namespace HonorCouncil_RazorPages.Models;

public class EvidenceFile
{
    public int Id { get; set; }

    public int HonorCaseId { get; set; }
    public HonorCase HonorCase { get; set; } = null!;

    [Required, StringLength(255)]
    public string OriginalFileName { get; set; } = string.Empty;

    [Required, StringLength(255)]
    public string StoredFileName { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string ContentType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public DateTime UploadedUtc { get; set; } = DateTime.UtcNow;

    [StringLength(200)]
    public string UploadedByDisplayName { get; set; } = string.Empty;

    [StringLength(50)]
    public string UploadedByRole { get; set; } = string.Empty;
}
