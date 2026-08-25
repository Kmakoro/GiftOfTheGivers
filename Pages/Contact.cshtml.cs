using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Pages;

public class ContactModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public ContactModel(ApplicationDbContext db) => _db = db;

    [BindProperty] public ContactInput Input { get; set; } = new();
    [TempData] public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        _db.ContactMessages.Add(new ContactMessage
        {
            FullName = Input.FullName.Trim(),
            Email = Input.Email.Trim(),
            Phone = Input.Phone?.Trim(),
            Subject = Input.Subject.Trim(),
            Message = Input.Message.Trim(),
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        SuccessMessage = "Thank you. Your message has been received and stored successfully.";
        return RedirectToPage();
    }

    public class ContactInput
    {
        [Required, Display(Name="Full name"), StringLength(160)] public string FullName { get; set; } = string.Empty;
        [Required, EmailAddress, StringLength(200)] public string Email { get; set; } = string.Empty;
        [Phone, StringLength(40)] public string? Phone { get; set; }
        [Required, StringLength(120)] public string Subject { get; set; } = string.Empty;
        [Required, StringLength(2000), MinLength(10)] public string Message { get; set; } = string.Empty;
    }
}
