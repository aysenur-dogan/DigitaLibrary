using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DigitaLibrary.Data;              // AppDbContext
using DigitaLibrary.Models;            // Admin, Post, AcademicWork
using DigitaLibrary.ViewModels;        // ProfilePageViewModel, ProfileEditViewModel
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

            // Yazılar
            var posts = await _db.Set<Post>()
                                 .AsNoTracking()
                                 .Where(p => p.AuthorId == me.Id)
                                 .OrderByDescending(p => p.CreatedAt)
                                 .ToListAsync();

            // Akademik Çalışmalar (profilde kart olarak göstereceğiz - son 8 kayıt)
            var myWorks = await _db.AcademicWorks
                                   .AsNoTracking()
                                   .Where(a => a.AuthorId == me.Id)
                                   .OrderByDescending(a => a.CreatedAt)
                                   .Take(8)
                                   .ToListAsync();

            var vm = new ProfilePageViewModel
            {
                User = me,
                MyPosts = posts,
                TotalPosts = posts.Count,
                PublishedPosts = posts.Count(p => p.IsPublished),
                DraftPosts = posts.Count(p => !p.IsPublished),

                // >>> EK: Profilde kartlar için
                MyWorks = myWorks
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return RedirectToAction(nameof(Me));

            // DisplayName'i basitçe ilk boşluğa göre bölüyoruz
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
                UserName = me.UserName,
                Email = me.Email,
                AvatarPath = me.AvatarPath,
                CoverPath = me.CoverPath,
                Bio = me.Bio,
                EducationInfo = me.EducationInfo,
                Interests = me.Interests
            };
            return View(vm);
        }

        // === DÜZENLE (POST) ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileEditViewModel vm)
        {
            Console.WriteLine($"[DEBUG] POST Education='{vm.EducationInfo}' Interests='{vm.Interests}'");

            var me = await _userManager.GetUserAsync(User);
            if (me == null) return RedirectToAction(nameof(Me));

            // Basit doğrulamalar
            if (string.IsNullOrWhiteSpace(vm.FirstName))
                ModelState.AddModelError(nameof(vm.FirstName), "İsim gereklidir.");

            // Dosya doğrulamaları
            if (vm.AvatarFile is { Length: > 0 } && !IsImage(vm.AvatarFile))
                ModelState.AddModelError(nameof(vm.AvatarFile), "jpg, png, webp veya gif (≤ 6 MB) yükleyin.");

            if (vm.CoverFile is { Length: > 0 } && !IsImage(vm.CoverFile))
                ModelState.AddModelError(nameof(vm.CoverFile), "jpg, png, webp veya gif (≤ 6 MB) yükleyin.");

            if (!ModelState.IsValid)
            {
                // Mevcut görsel yollarını koru
                vm.AvatarPath = me.AvatarPath;
                vm.CoverPath = me.CoverPath;
                return View(vm);
            }

            // İsim–soyisim → DisplayName
            var display = $"{vm.FirstName} {vm.LastName}".Trim();
            me.DisplayName = string.IsNullOrWhiteSpace(display) ? vm.FirstName?.Trim() : display;

            // Biyografi
            me.Bio = vm.Bio?.Trim();
            me.EducationInfo = vm.EducationInfo?.Trim();
            me.Interests = vm.Interests?.Trim();

            // Kullanıcı adı değiştiyse
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

            // E-posta değiştiyse
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

            // Avatar kaydet
            if (vm.AvatarFile is { Length: > 0 })
            {
                var path = await SaveImage(vm.AvatarFile, me.Id, "avatars");
                DeleteIfExists(me.AvatarPath);
                me.AvatarPath = path;
            }

            // Kapak kaydet
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

        // ===== Helpers (controller İÇİNDE olmalı) =====
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

        // === Akademik çalışmaların TAM listesi (profil sekmesindeki buton buraya gider) ===
        public async Task<IActionResult> MyWorks()
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return Challenge();

            var works = await _db.AcademicWorks
                .Where(x => x.AuthorId == me.Id)
                .OrderByDescending(x => x.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            return View(works); // Views/Profile/MyWorks.cshtml
        }
    }
}
