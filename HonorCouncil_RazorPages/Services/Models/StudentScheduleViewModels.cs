namespace HonorCouncil_RazorPages.Services.Models;

public class StudentScheduleItemViewModel
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public string UploadedUtcDisplay { get; set; } = string.Empty;
}
