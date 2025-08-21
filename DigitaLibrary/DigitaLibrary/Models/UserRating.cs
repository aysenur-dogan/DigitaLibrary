using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace DigitaLibrary.Models
{
    public class UserRating
    {
        public int Id { get; set; }

        [Required] public string RaterId { get; set; } = default!;      // Oyu veren (Admin.Id)
        [Required] public string RatedUserId { get; set; } = default!;  // Oyu alan (Admin.Id)

        [Range(1, 5)]
        public byte Score { get; set; }                                 // 1..5

        [MaxLength(2000)]
        public string? Comment { get; set; }                            // opsiyonel yorum

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // (İsteğe bağlı) navigationlar:
        public Admin? Rater { get; set; }       // oyu veren kullanıcı
        public Admin? RatedUser { get; set; }   // oyu alan kullanıcı
    }
}
