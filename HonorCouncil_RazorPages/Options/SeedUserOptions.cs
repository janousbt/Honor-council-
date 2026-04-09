namespace HonorCouncil_RazorPages.Options;

public sealed class SeedUserOptions
{
    public const string SectionName = "SeedUsers";

    public List<SeedUserAccount> Accounts { get; set; } = [];
}

public sealed class SeedUserAccount
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
