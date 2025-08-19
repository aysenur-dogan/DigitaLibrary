using System.Text.RegularExpressions;
using DigitaLibrary.Data;
using DigitaLibrary.Models;
using DigitaLibrary.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace DigitaLibrary.Controllers
{
    [Authorize]
    public class AcademicWorksController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<Admin> _user;
        private readonly IWebHostEnvironment _env;

        private const long MaxBytes = 20 * 1024 * 1024; // 20MB
        private static readonly string[] AllowedDocMime = {
            "application/pdf",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        };
        private static readonly string[] AllowedImageMime = {
            "image/jpeg","image/png","image/webp","image/gif"
        };

        public AcademicWorksController(AppDbContext db, UserManager<Admin> user, IWebHostEnvironment env)
        { _db = db; _user = user; _env = env; }

        // Liste
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var list = await _db.AcademicWorks
                .Include(a => a.Category)
                .OrderByDescending(a => a.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
            return View(list);
        }

        // Detay
        [AllowAnonymous]
        public async Task<IActionResult> Details(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return NotFound();
            var w = await _db.AcademicWorks
                .Include(a => a.Category)
                .Include(a => a.Author)
                .FirstOrDefaultAsync(a => a.Slug == slug);
            return w == null ? NotFound() : View(w);
        }

        // Ekle (GET)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
            return View(new AcademicWorkCreateVm());
        }

        // Ekle (POST)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AcademicWorkCreateVm vm)
        {
            var me = await _user.GetUserAsync(User);
            if (me == null) return Challenge();

            // "en az bir içerik" kuralı
            if (vm.ContentMode == ContentMode.FileOnly && (vm.File == null || vm.File.Length == 0))
                ModelState.AddModelError(nameof(vm.File), "Dosya yükleyin veya içerik türünü değiştirin.");
            if (vm.ContentMode == ContentMode.TextOnly && string.IsNullOrWhiteSpace(vm.HtmlContent))
                ModelState.AddModelError(nameof(vm.HtmlContent), "Metin içeriği girin veya içerik türünü değiştirin.");

            // Dosya doğrulama
            if (vm.File is { Length: > 0 })
            {
                if (!AllowedDocMime.Contains(vm.File.ContentType))
                    ModelState.AddModelError(nameof(vm.File), "Sadece PDF/DOCX yüklenebilir.");
                if (vm.File.Length > MaxBytes)
                    ModelState.AddModelError(nameof(vm.File), "Dosya boyutu 20MB'ı aşamaz.");
            }

            // Kapak resmi doğrulama
            if (vm.CoverImage is { Length: > 0 } && !AllowedImageMime.Contains(vm.CoverImage.ContentType))
                ModelState.AddModelError(nameof(vm.CoverImage), "Kapak resmi için JPG/PNG/WEBP/GIF kullanın.");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
                return View(vm);
            }

            // Slug
            var slug = Slugify(string.IsNullOrWhiteSpace(vm.Slug) ? vm.Title : vm.Slug);
            slug = await MakeUniqueSlug(slug);

            // PDF/DOCX kaydet
            string? filePath = null, fileMime = null; long? fileSize = null;
            if (vm.File is { Length: > 0 })
            {
                var year = DateTime.UtcNow.Year.ToString();
                var dir = Path.Combine(_env.WebRootPath, "uploads", "works", year);
                Directory.CreateDirectory(dir);

                var name = $"{Guid.NewGuid():N}{Path.GetExtension(vm.File.FileName).ToLowerInvariant()}";
                var full = Path.Combine(dir, name);
                await using (var fs = new FileStream(full, FileMode.Create))
                    await vm.File.CopyToAsync(fs);

                filePath = $"/uploads/works/{year}/{name}";
                fileMime = vm.File.ContentType;
                fileSize = vm.File.Length;
            }

            // Kapak kaydet
            string? coverPath = null;
            if (vm.CoverImage is { Length: > 0 })
            {
                var dir = Path.Combine(_env.WebRootPath, "uploads", "covers");
                Directory.CreateDirectory(dir);

                var name = $"{Guid.NewGuid():N}{Path.GetExtension(vm.CoverImage.FileName).ToLowerInvariant()}";
                var full = Path.Combine(dir, name);
                await using (var fs = new FileStream(full, FileMode.Create))
                    await vm.CoverImage.CopyToAsync(fs);

                coverPath = $"/uploads/covers/{name}";
            }

            // (İsteğe bağlı) HTML sanitize – paket ekleyince aktif edersin
            string? cleanHtml = string.IsNullOrWhiteSpace(vm.HtmlContent) ? null : vm.HtmlContent;

            var a = new AcademicWork
            {
                Title = vm.Title.Trim(),
                Slug = slug,
                Authors = vm.Authors?.Trim(),
                AuthorTitle = vm.AuthorTitle?.Trim(),
                Institution = vm.Institution?.Trim(),
                Year = vm.Year,
                Supervisor = vm.Supervisor?.Trim(),
                Keywords = vm.Keywords?.Trim(),
                PublicationType = vm.PublicationType,
                Language = vm.Language,
                Abstract = vm.Abstract.Trim(),
                CategoryId = vm.CategoryId,
                AuthorId = me.Id,
                ContentMode = vm.ContentMode,
                HtmlContent = cleanHtml,
                FilePath = filePath,
                FileType = fileMime,
                FileSize = fileSize,
                CoverImagePath = coverPath,
                IsApproved = true
            };

            _db.AcademicWorks.Add(a);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { slug = a.Slug });
        }

        // === helpers ===
        private static string Slugify(string input)
        {
            input = input.Trim().ToLowerInvariant()
                .Replace("ğ", "g").Replace("ü", "u").Replace("ş", "s")
                .Replace("ı", "i").Replace("ö", "o").Replace("ç", "c");
            input = Regex.Replace(input, @"[^a-z0-9\s-]", "");
            input = Regex.Replace(input, @"\s+", "-");
            input = Regex.Replace(input, @"-+", "-");
            return input.Trim('-');
        }

        private async Task<string> MakeUniqueSlug(string slug)
        {
            var baseSlug = slug; var n = 1;
            while (await _db.AcademicWorks.AnyAsync(x => x.Slug == slug))
                slug = $"{baseSlug}-{++n}";
            return slug;

        }
        // Düzenle (GET)
        // Düzenle (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var work = await _db.AcademicWorks.FindAsync(id);
            if (work == null) return NotFound();

            var vm = new AcademicWorkCreateVm
            {
                Title = work.Title,
                Slug = work.Slug,
                Authors = work.Authors,
                AuthorTitle = work.AuthorTitle,
                Institution = work.Institution,
                Year = work.Year,
                Supervisor = work.Supervisor,
                Keywords = work.Keywords,
                PublicationType = work.PublicationType,
                Language = work.Language,
                Abstract = work.Abstract,
                CategoryId = work.CategoryId,
                ContentMode = work.ContentMode,
                HtmlContent = work.HtmlContent
            };

            ViewBag.Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
            return View(vm);
        }

        // Düzenle (POST)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AcademicWorkCreateVm vm)
        {
            var work = await _db.AcademicWorks.FindAsync(id);
            if (work == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
                return View(vm);
            }

            // Slug güncelle (boş değilse)
            if (!string.IsNullOrWhiteSpace(vm.Slug))
                work.Slug = vm.Slug.Trim();

            // Metin alanlarını güncelle
            work.Title = vm.Title.Trim();
            work.Authors = vm.Authors?.Trim();
            work.AuthorTitle = vm.AuthorTitle?.Trim();
            work.Institution = vm.Institution?.Trim();
            work.Year = vm.Year;
            work.Supervisor = vm.Supervisor?.Trim();
            work.Keywords = vm.Keywords?.Trim();
            work.PublicationType = vm.PublicationType;
            work.Language = vm.Language;
            work.Abstract = vm.Abstract.Trim();
            work.CategoryId = vm.CategoryId;
            work.ContentMode = vm.ContentMode;
            work.HtmlContent = vm.HtmlContent?.Trim();
            work.UpdatedAt = DateTime.UtcNow;

            // === Dosya güncelle ===
            if (vm.File is { Length: > 0 })
            {
                // Eski dosyayı sil
                if (!string.IsNullOrWhiteSpace(work.FilePath))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, work.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                var year = DateTime.UtcNow.Year.ToString();
                var dir = Path.Combine(_env.WebRootPath, "uploads", "works", year);
                Directory.CreateDirectory(dir);

                var name = $"{Guid.NewGuid():N}{Path.GetExtension(vm.File.FileName).ToLowerInvariant()}";
                var full = Path.Combine(dir, name);
                await using (var fs = new FileStream(full, FileMode.Create))
                    await vm.File.CopyToAsync(fs);

                work.FilePath = $"/uploads/works/{year}/{name}";
                work.FileType = vm.File.ContentType;
                work.FileSize = vm.File.Length;
            }

            // === Kapak güncelle ===
            if (vm.CoverImage is { Length: > 0 })
            {
                if (!string.IsNullOrWhiteSpace(work.CoverImagePath))
                {
                    var oldCover = Path.Combine(_env.WebRootPath, work.CoverImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldCover))
                        System.IO.File.Delete(oldCover);
                }

                var dir = Path.Combine(_env.WebRootPath, "uploads", "covers");
                Directory.CreateDirectory(dir);

                var name = $"{Guid.NewGuid():N}{Path.GetExtension(vm.CoverImage.FileName).ToLowerInvariant()}";
                var full = Path.Combine(dir, name);
                await using (var fs = new FileStream(full, FileMode.Create))
                    await vm.CoverImage.CopyToAsync(fs);

                work.CoverImagePath = $"/uploads/covers/{name}";
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { slug = work.Slug });
        }


    }
}
