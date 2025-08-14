using Microsoft.AspNetCore.Identity;

namespace DigitaLibrary.Models
{
    public class Admin : IdentityUser
    {
        public string? DisplayName { get; set; }
        public string? AvatarPath { get; set; }
    }
}
