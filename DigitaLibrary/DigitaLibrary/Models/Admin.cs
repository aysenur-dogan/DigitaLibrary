// Admin.cs
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

public class Admin : IdentityUser
{
    public string? DisplayName { get; set; }
    public string? AvatarPath { get; set; }

    // [NotMapped]  // ← BUNU KALDIR
    public string? CoverPath { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }

    // YENİ
    [MaxLength(200)]
    public string? EducationInfo { get; set; }   // Eğitim Bilgileri
    [MaxLength(200)]
    public string? Interests { get; set; }
    public string? FirstName { get; set; }   // yeni
    public string? LastName { get; set; }    // yeni// İlgi Alanları (virgüllü metin olabilir)
}
