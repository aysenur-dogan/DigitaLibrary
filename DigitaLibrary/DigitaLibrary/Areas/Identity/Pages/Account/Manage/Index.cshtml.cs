using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DigitaLibrary.Models; // Admin modelin burada

public class IndexModel : PageModel
{
    private readonly UserManager<Admin> _userManager;

    public IndexModel(UserManager<Admin> userManager)
    {
        _userManager = userManager;
    }

    public string Username { get; set; } // @Model.Username için

    [BindProperty]
    public InputModel Input { get; set; } // @Model.Input.Email için

    public class InputModel
    {
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Telefon Numarası")]
        public string PhoneNumber { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("Kullanıcı bulunamadı.");
        }

        Username = user.UserName;

        Input = new InputModel
        {
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        };

        return Page();
    }
}
