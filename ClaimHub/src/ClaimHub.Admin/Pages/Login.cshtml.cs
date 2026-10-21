using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using ClaimHub.Admin.Infrastructure;
using ClaimHub.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ClaimHub.Admin.Pages;

public class LoginModel : PageModel
{
    private readonly ClaimHubDbContext _db;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(ClaimHubDbContext db, ILogger<LoginModel> logger)
    {
        _db = db;
        _logger = logger;
    }

    [BindProperty]
    public LoginInput Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true && string.IsNullOrEmpty(ReturnUrl))
        {
            return RedirectToPage("/Dashboard");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == Input.Email);
        if (user == null)
        {
            _logger.LogWarning("Sign-in attempt for unknown user {Email}", Input.Email);
            ModelState.AddModelError(string.Empty, "User not found.");
            return Page();
        }

        if (!PasswordVerifier.Verify(Input.Password, user.PasswordHash))
        {
            _logger.LogWarning("Wrong password for user {Email}", Input.Email);
            ModelState.AddModelError(string.Empty, "Wrong password.");
            return Page();
        }

        if (!user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "This account is disabled.");
            return Page();
        }

        user.RecordLogin(DateTimeOffset.UtcNow);
        await _db.SaveChangesAsync();

        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim("DisplayName", user.DisplayName),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            },
            CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = Input.RememberMe });

        if (!string.IsNullOrEmpty(ReturnUrl))
        {
            return Redirect(ReturnUrl);
        }

        return RedirectToPage("/Dashboard");
    }

    public class LoginInput
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Keep me signed in")]
        public bool RememberMe { get; set; }
    }
}
