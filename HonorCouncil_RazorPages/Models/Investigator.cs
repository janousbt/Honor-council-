using System.ComponentModel.DataAnnotations;

namespace HonorCouncil_RazorPages.Models;

public class Investigator
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(320)]
    public string Email { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<HonorCase> AssignedCases { get; set; } = new List<HonorCase>();
}
