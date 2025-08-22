using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using DigitaLibrary.Models;

namespace DigitaLibrary.ViewModels
{
    // EF Core'un bu sınıfı tablo sanmaması için:
    [NotMapped]
    public class ProfilePageViewModel
    {
        public IEnumerable<AcademicWork> MyWorks { get; set; } = new List<AcademicWork>();
        public Admin User { get; set; } = default!;
        public IEnumerable<Post> MyPosts { get; set; } = new List<Post>();

        public int TotalPosts { get; set; }
        public int PublishedPosts { get; set; }
        public int DraftPosts { get; set; }
        public double AverageScore { get; set; }
        public int RatingsCount { get; set; }
      
        public bool IsOwner { get; set; }   // bu kullanıcı sayfanın sahibi mi?

    }
}
