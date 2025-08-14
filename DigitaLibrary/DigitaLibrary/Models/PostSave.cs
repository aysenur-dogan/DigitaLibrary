namespace DigitaLibrary.Models
{
    public class PostSave
    {
        public string UserId { get; set; } = default!;
        public Admin? User { get; set; }   // Kimlik sınıfın farklıysa (örn. ApplicationUser) buna göre değiştir

        public int PostId { get; set; }
        public Post? Post { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
