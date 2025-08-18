using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

public class Admin : IdentityUser
{
    public string? DisplayName { get; set; }
    public string? AvatarPath { get; set; }

    [NotMapped]                 // ← EF bu alanı tabloya beklemez
    public string? CoverPath { get; set; }
}
