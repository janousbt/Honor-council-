using System.ComponentModel.DataAnnotations;

namespace HonorCouncil_RazorPages.Models;

public class ApplicationUser
{
    public int Id { get; set; }

    [Required, EmailAddress, StringLength(320)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Role { get; set; } = string.Empty;

    [Required, StringLength(1000)]
    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
}
