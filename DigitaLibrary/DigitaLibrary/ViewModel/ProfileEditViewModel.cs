using Microsoft.AspNetCore.Http;

namespace DigitaLibrary.ViewModels
{
    // Profil düzenleme (display name + e-posta + avatar/kapak yükleme)
    public class ProfileEditViewModel
    {
        public string? DisplayName { get; set; }
        public string? Email { get; set; }

        // Önizleme için mevcut yollar
        public string? AvatarPath { get; set; }
        public string? CoverPath { get; set; }

        // Yüklenecek dosyalar
        public IFormFile? AvatarFile { get; set; }
        public IFormFile? CoverFile { get; set; }
    }
}
