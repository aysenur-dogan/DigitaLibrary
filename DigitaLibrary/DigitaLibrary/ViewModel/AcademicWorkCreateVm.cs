using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using DigitaLibrary.Models;

namespace DigitaLibrary.ViewModels
{
    public class AcademicWorkCreateVm
    {
        [Display(Name = "Başlık"), Required, StringLength(180)]
        public string Title { get; set; } = default!;

        [Display(Name = "Slug (URL)"), StringLength(200)]
        public string? Slug { get; set; }

        [Display(Name = "Yazar(lar)"), StringLength(255)]
        public string? Authors { get; set; }

        [Display(Name = "Yazar Unvanı"), StringLength(80)]
        public string? AuthorTitle { get; set; }

        [Display(Name = "Kurum"), StringLength(255)]
        public string? Institution { get; set; }

        [Display(Name = "Yıl"), Range(1900, 2100)]
        public int? Year { get; set; }

        [Display(Name = "Danışman"), StringLength(255)]
        public string? Supervisor { get; set; }

        [Display(Name = "Anahtar Kelimeler"), StringLength(500)]
        public string? Keywords { get; set; }

        [Display(Name = "Yayın Türü"), Required]
        public PublicationType PublicationType { get; set; } = PublicationType.Makale;

        [Display(Name = "Dil"), Required]
        public WorkLanguage Language { get; set; } = WorkLanguage.Turkish;

        [Display(Name = "Özet"), Required, StringLength(2000)]
        public string Abstract { get; set; } = default!;

        [Display(Name = "Kategori")]
        public int? CategoryId { get; set; }

        [Display(Name = "İçerik Türü")]
        public ContentMode ContentMode { get; set; } = ContentMode.FileOnly;

        [Display(Name = "Sitede Yaz (Zengin Metin)")]
        public string? HtmlContent { get; set; }

        [Display(Name = "PDF veya DOCX")]
        public IFormFile? File { get; set; }

        [Display(Name = "Kapak Resmi")]
        public IFormFile? CoverImage { get; set; }
    }
}
