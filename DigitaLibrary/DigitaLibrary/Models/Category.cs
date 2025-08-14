using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;

namespace DigitaLibrary.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, MaxLength(64)]
        public string Name { get; set; } = default!;

        [Required, MaxLength(64)]
        public string Slug { get; set; } = default!; // url-dostu isim (örn: "egitim")

        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}
