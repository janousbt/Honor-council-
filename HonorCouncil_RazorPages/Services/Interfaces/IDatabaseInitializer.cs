namespace HonorCouncil_RazorPages.Services.Interfaces;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
