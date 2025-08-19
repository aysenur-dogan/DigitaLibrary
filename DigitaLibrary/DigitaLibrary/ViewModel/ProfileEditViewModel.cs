// ProfileEditViewModel.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DigitaLibrary.ViewModels
{
    public class ProfileEditViewModel
    {
        // Görsel alanlar (var olanlar)
        public string? AvatarPath { get; set; }
        public string? CoverPath { get; set; }
        public IFormFile? AvatarFile { get; set; }
        public IFormFile? CoverFile { get; set; }

        // Yeni: kullanıcı bilgileri formu
        [Display(Name = "İsim")]
        [MaxLength(50)]
        public string? FirstName { get; set; }

        [Display(Name = "Soyisim")]
        [MaxLength(50)]
        public string? LastName { get; set; }

        [Display(Name = "Kullanıcı Adı")]
        [MaxLength(64)]
        public string? UserName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Display(Name = "Biyografi")]
        [MaxLength(500, ErrorMessage = "Biyografi en fazla 500 karakter olabilir.")]
        public string? Bio { get; set; }
        // ProfileEditViewModel.cs
        [Display(Name = "Eğitim Bilgileri")]
        [MaxLength(200)]
        public string? EducationInfo { get; set; }

        [Display(Name = "İlgi Alanları")]
        [MaxLength(200)]
        public string? Interests { get; set; }



    }
}
