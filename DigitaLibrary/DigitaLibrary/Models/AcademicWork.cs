using System;
using System.ComponentModel.DataAnnotations;

namespace DigitaLibrary.Models
{
    public enum PublicationType { Makale = 1, Tez = 2, Bildiri = 3, Rapor = 4 }
    public enum WorkLanguage { Turkish = 1, English = 2, Other = 3 }
    public enum ContentMode { FileOnly = 1, TextOnly = 2, FileAndText = 3 }

    public class AcademicWork
    {
        public int Id { get; set; }

        [Required, StringLength(180)]
        public string Title { get; set; } = default!;             // Başlık

        [Required, StringLength(200)]
        public string Slug { get; set; } = default!;              // URL

        [StringLength(255)]
        public string? Authors { get; set; }                      // "Ad Soyad, Ad Soyad"

        [StringLength(80)]
        public string? AuthorTitle { get; set; }                  // Örn: Prof. Dr., Öğr. Gör., Öğrenci

        [StringLength(255)]
        public string? Institution { get; set; }                  // Kurum

        [Range(1900, 2100)]
        public int? Year { get; set; }

        [StringLength(255)]
        public string? Supervisor { get; set; }                   // Danışman

        [StringLength(500)]
        public string? Keywords { get; set; }

        [Required]
        public PublicationType PublicationType { get; set; } = PublicationType.Makale;

        [Required]
        public WorkLanguage Language { get; set; } = WorkLanguage.Turkish;

        [Required, StringLength(2000)]
        public string Abstract { get; set; } = default!;          // Özet

        // İçerik modu + sitede yazılan HTML
        public ContentMode ContentMode { get; set; } = ContentMode.FileOnly;

        [StringLength(100000)]
        public string? HtmlContent { get; set; }                  // Sitede yaz

        // Dosya bilgileri (PDF/DOCX)
        [StringLength(500)] public string? FilePath { get; set; }
        [StringLength(100)] public string? FileType { get; set; }
        public long? FileSize { get; set; }

        // KAPAK RESMİ
        [StringLength(500)] public string? CoverImagePath { get; set; }

        // İlişkiler
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        [Required] public string AuthorId { get; set; } = default!;
        public Admin Author { get; set; } = default!;

        // Yönetimsel
        public bool IsApproved { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
