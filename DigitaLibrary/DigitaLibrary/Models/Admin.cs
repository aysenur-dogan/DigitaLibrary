using Microsoft.AspNetCore.Identity;

namespace YourNamespaceHere.Models // Burada kendi proje namespace'ini kullan
{
    public class Admin : IdentityUser
    {
        public string? DisplayName { get; set; }
        public string? AvatarPath { get; set; }
    }
}
