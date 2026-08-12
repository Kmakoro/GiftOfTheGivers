using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GiftOfTheGivers.Pages;

public class ContactModel : PageModel
{
    [BindProperty]
    public ContactInput Input { get; set; } = new();

    public void OnGet() { }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        TempData["Success"] = $"Thank you {Input.Name}, your message has been received. Our team will respond shortly.";
        return RedirectToPage();
    }

    public class ContactInput
    {
        [Required, MaxLength(120)]
        [Display(Name = "Your name")]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(200)]
        [Display(Name = "Email address")]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Subject { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string Message { get; set; } = string.Empty;
    }
}
