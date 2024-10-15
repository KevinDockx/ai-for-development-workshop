using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClaimHub.Admin.Pages;

public class IndexModel : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Dashboard");
}
