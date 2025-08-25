using DigitaLibrary.Data;              // AppDbContext
using DigitaLibrary.Models;            // Admin, Post, AcademicWork
using DigitaLibrary.ViewModels;        // ProfilePageViewModel, ProfileEditViewModel
using Ganss.Xss;
 // ← EKLENDİ
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DigitaLibrary.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<Admin> _userManager;
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ProfileController(UserManager<Admin> userManager, AppDbContext db, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _db = db;
            _env = env;
        }

        // === PROFİL SAYFASI ===
        [HttpGet]
        public async Task<IActionResult> Me()
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return Challenge();

            // SADECE AcademicWorks'den çek
            var myWorks = await _db.AcademicWorks
                .AsNoTracking()
                .Where(a => a.AuthorId == me.Id)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            // sayaçlar
            var total = myWorks.Count;
            var published = myWorks.Count(x => x.IsApproved);   // Yayında
            var drafts = total - published;                     // Taslak

            var vm = new ProfilePageViewModel
            {
                User = me,
                MyWorks = myWorks.Take(8).ToList(), // kart gridinde gösterilecekler
                TotalPosts = total,
                PublishedPosts = published,
                DraftPosts = drafts,
                IsOwner = true
            };

            return View(vm);
        }


        // === BAŞKA BİR KULLANICININ PROFİLİNİ GÖRÜNTÜLE ===
        // ProfileController.cs
        [AllowAnonymous]
        [HttpGet("Profile/View/{id}")]
        public async Task<IActionResult> ViewProfile(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var viewer = await _userManager.GetUserAsync(User);

            var works = await _db.AcademicWorks
                .AsNoTracking()
                .Where(x => x.AuthorId == id)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            // sayaçlar (başkasının profili olsa da aynı mantık)
            var total = works.Count;
            var published = works.Count(x => x.IsApproved);
            var drafts = total - published;

            var vm = new ProfilePageViewModel
            {
                User = user,
                MyWorks = works.Take(8).ToList(),
                TotalPosts = total,
                PublishedPosts = published,
                DraftPosts = drafts,
                IsOwner = (viewer?.Id == id)
            };

            return View("Me", vm); // aynı view
        }



        // === PROFİL DÜZENLEME (GET) ===
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return RedirectToAction(nameof(Me));

            var first = me.DisplayName;
            var last = "";
            if (!string.IsNullOrWhiteSpace(me.DisplayName))
            {
                var parts = me.DisplayName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                first = parts.ElementAtOrDefault(0) ?? "";
                last = parts.ElementAtOrDefault(1) ?? "";
            }

            var vm = new ProfileEditViewModel
            {
                FirstName = first,
                LastName = last,
                UserName = me.UserName ?? string.Empty,
                Email = me.Email ?? string.Empty,
                AvatarPath = me.AvatarPath,
                CoverPath = me.CoverPath,
                Bio = me.Bio,
                EducationInfo = me.EducationInfo,
                Interests = me.Interests
            };
            return View(vm);
        }

        // === PROFİL DÜZENLEME (POST) ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileEditViewModel vm)
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return RedirectToAction(nameof(Me));

            if (string.IsNullOrWhiteSpace(vm.FirstName))
                ModelState.AddModelError(nameof(vm.FirstName), "İsim gereklidir.");

            if (vm.AvatarFile is { Length: > 0 } && !IsImage(vm.AvatarFile))
                ModelState.AddModelError(nameof(vm.AvatarFile), "jpg, png, webp veya gif (≤ 6 MB) yükleyin.");

            if (vm.CoverFile is { Length: > 0 } && !IsImage(vm.CoverFile))
                ModelState.AddModelError(nameof(vm.CoverFile), "jpg, png, webp veya gif (≤ 6 MB) yükleyin.");

            if (!ModelState.IsValid)
            {
                vm.AvatarPath = me.AvatarPath;
                vm.CoverPath = me.CoverPath;
                return View(vm);
            }

            // ---- BIO'yu CKEditor'den gelen HTML ile güvenli kaydet ----
            var sanitizer = new HtmlSanitizer();

            // CKEditor’un kullandığı <span style="..."> gibi etiket/özellikleri açıyoruz
            sanitizer.AllowedTags.Add("span");
            sanitizer.AllowedAttributes.Add("style");

            sanitizer.AllowedCssProperties.Add("font-size");
            sanitizer.AllowedCssProperties.Add("font-family");
            sanitizer.AllowedCssProperties.Add("color");
            sanitizer.AllowedCssProperties.Add("background-color");
            sanitizer.AllowedCssProperties.Add("text-align");

            var cleanBio = sanitizer.Sanitize(vm.Bio ?? string.Empty).Trim();

            // İsim / Soyisim
            var display = $"{vm.FirstName} {vm.LastName}".Trim();
            me.DisplayName = string.IsNullOrWhiteSpace(display) ? vm.FirstName?.Trim() : display;

            // Alanları güncelle
            me.Bio = cleanBio;
            me.EducationInfo = vm.EducationInfo?.Trim();
            me.Interests = vm.Interests?.Trim();

            // Kullanıcı adı değişikliği
            if (!string.IsNullOrWhiteSpace(vm.UserName) && vm.UserName != me.UserName)
            {
                var setUserName = await _userManager.SetUserNameAsync(me, vm.UserName.Trim());
                if (!setUserName.Succeeded)
                {
                    foreach (var e in setUserName.Errors)
                        ModelState.AddModelError(nameof(vm.UserName), e.Description);

                    vm.AvatarPath = me.AvatarPath;
                    vm.CoverPath = me.CoverPath;
                    return View(vm);
                }
            }

            // Email değişikliği
            if (!string.IsNullOrWhiteSpace(vm.Email) && vm.Email != me.Email)
            {
                var setEmail = await _userManager.SetEmailAsync(me, vm.Email.Trim());
                if (!setEmail.Succeeded)
                {
                    foreach (var e in setEmail.Errors)
                        ModelState.AddModelError(nameof(vm.Email), e.Description);

                    vm.AvatarPath = me.AvatarPath;
                    vm.CoverPath = me.CoverPath;
                    return View(vm);
                }
            }

            // Profil fotoğrafı
            if (vm.AvatarFile is { Length: > 0 })
            {
                var path = await SaveImage(vm.AvatarFile, me.Id, "avatars");
                DeleteIfExists(me.AvatarPath);
                me.AvatarPath = path;
            }

            // Kapak fotoğrafı
            if (vm.CoverFile is { Length: > 0 })
            {
                var path = await SaveImage(vm.CoverFile, me.Id, "covers");
                DeleteIfExists(me.CoverPath);
                me.CoverPath = path;
            }

            await _userManager.UpdateAsync(me);

            TempData["ok"] = "Kullanıcı bilgileri güncellendi.";
            return RedirectToAction(nameof(Me));
        }


        // === Yardımcılar ===
        private static bool IsImage(IFormFile f)
        {
            var ok = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            return ok.Contains(f.ContentType) && f.Length <= 6_000_000;
        }

        private async Task<string> SaveImage(IFormFile file, string userId, string subFolder)
        {
            var dir = Path.Combine(_env.WebRootPath, "uploads", "users", userId, subFolder);
            Directory.CreateDirectory(dir);

            var name = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
            var full = Path.Combine(dir, name);
            await using var fs = new FileStream(full, FileMode.Create);
            await file.CopyToAsync(fs);

            return $"/uploads/users/{userId}/{subFolder}/{name}";
        }

        private void DeleteIfExists(string? webPath)
        {
            if (string.IsNullOrWhiteSpace(webPath)) return;
            var full = Path.Combine(_env.WebRootPath, webPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(full)) System.IO.File.Delete(full);
        }

        // === Akademik Çalışmalar Listesi ===
        public async Task<IActionResult> MyWorks()
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return Challenge();

            var works = await _db.AcademicWorks
                .Where(x => x.AuthorId == me.Id)
                .OrderByDescending(x => x.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            return View(works);
        }

    }
}
