using HonorCouncil_RazorPages.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HonorCouncil_RazorPages.Pages;

public class IndexModel(ICurrentUserService currentUserService) : PageModel
{
    public IActionResult OnGet()
    {
        if (!currentUserService.IsAuthenticated)
        {
            return RedirectToPage("/Admin/Login");
        }

        if (currentUserService.IsInRole("Student"))
        {
            return RedirectToPage("/Student/Cases/Index");
        }

        if (currentUserService.IsInRole("Admin", "President", "Coordinator", "Investigator"))
        {
            return RedirectToPage("/Admin/Dashboard");
        }

        return Page();
    }
}
