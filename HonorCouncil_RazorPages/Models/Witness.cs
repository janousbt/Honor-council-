using System.ComponentModel.DataAnnotations;

namespace HonorCouncil_RazorPages.Models;

public class Witness
{
    public int Id { get; set; }

    public int HonorCaseId { get; set; }
    public HonorCase HonorCase { get; set; } = null!;

    [Required, StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress, StringLength(320)]
    public string? Email { get; set; }

    [StringLength(150)]
    public string? Affiliation { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public DateTime? LastNotifiedUtc { get; set; }
}
