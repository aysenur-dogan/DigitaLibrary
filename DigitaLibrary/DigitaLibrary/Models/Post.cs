using System.ComponentModel.DataAnnotations;

namespace DigitaLibrary.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required, MaxLength(140)]
        public string Title { get; set; } = default!;

        [Required, MaxLength(160)]
        public string Slug { get; set; } = default!;

        [MaxLength(280)]
        public string? Excerpt { get; set; }

        [Required] // HTML içerik
        public string Content { get; set; } = default!;

        [MaxLength(512)]
        public string? ThumbnailUrl { get; set; }

        public bool IsPublished { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Kategori (V1: tek)
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!; // zorunlu ilişki niyeti

        // Yazar (Identity kullanıcın)
        [MaxLength(450)]                 // Identity anahtar uzunluğu
        public string AuthorId { get; set; } = default!;
        public Admin Author { get; set; } = null!;

        // Beğeni & Kaydetme (DbContext ile uyumlu)
        public ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
        public ICollection<PostSave> Saves { get; set; } = new List<PostSave>();
    }
}
