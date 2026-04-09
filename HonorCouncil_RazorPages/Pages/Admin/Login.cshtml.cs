using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using HonorCouncil_RazorPages.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HonorCouncil_RazorPages.Pages.Admin;

[AllowAnonymous]
public class LoginModel(IApplicationAuthenticationService authenticationService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var account = await authenticationService.ValidateCredentialsAsync(Input.Email, Input.Password, cancellationToken);
        if (account is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid credentials.");
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, account.DisplayName),
            new(ClaimTypes.Email, account.Email),
            new(ClaimTypes.Role, account.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return account.Role switch
        {
            "Student" => RedirectToPage("/Student/Cases/Index"),
            "Faculty" => RedirectToPage("/Index"),
            _ => RedirectToPage("/Admin/Dashboard")
        };
    }

    public class InputModel
    {
        [Required, EmailAddress, Display(Name = "JMU email")]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
