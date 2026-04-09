using HonorCouncil_RazorPages.Services.Interfaces;

namespace HonorCouncil_RazorPages.Services;

public class EvidenceService(IWebHostEnvironment environment) : IEvidenceService
{
    public string GetUploadRoot() => Path.Combine(environment.ContentRootPath, "App_Data", "Evidence");
}
