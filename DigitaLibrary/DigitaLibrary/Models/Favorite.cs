using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitaLibrary.Models
{
    public class Favorite
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int AcademicWorkId { get; set; }

        [ForeignKey(nameof(UserId))]
        public Admin? User { get; set; }

        [ForeignKey(nameof(AcademicWorkId))]
        public AcademicWork? Work { get; set; }
    }
}
