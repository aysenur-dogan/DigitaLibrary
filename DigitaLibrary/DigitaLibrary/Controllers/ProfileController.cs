using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DigitaLibrary.Data;              // AppDbContext
using DigitaLibrary.Models;            // Admin, Post
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

            var posts = await _db.Set<Post>()
                                 .AsNoTracking()
                                 .Where(p => p.AuthorId == me.Id)             // alan adları farklıysa uyarlayın
                                 .OrderByDescending(p => p.CreatedAt)
                                 .ToListAsync();

            var vm = new ProfilePageViewModel
            {
                User = me,
                MyPosts = posts,
                TotalPosts = posts.Count,
                PublishedPosts = posts.Count(p => p.IsPublished),
                DraftPosts = posts.Count(p => !p.IsPublished)
            };

            return View(vm);
        }

        // === DÜZENLE (GET) ===
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return RedirectToAction(nameof(Me));

            var vm = new ProfileEditViewModel
            {
                DisplayName = string.IsNullOrWhiteSpace(me.DisplayName) ? me.UserName : me.DisplayName,
                Email = me.Email,
                AvatarPath = me.AvatarPath,
                CoverPath = me.CoverPath
            };
            return View(vm);
        }

        // === DÜZENLE (POST) + DOSYA YÜKLE ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileEditViewModel vm)
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return RedirectToAction(nameof(Me));

            me.DisplayName = vm.DisplayName?.Trim();
            // E-postayı burada değiştirmek istemiyorsan bu satırı kaldır:
            me.Email = vm.Email?.Trim() ?? me.Email;

            // Avatar
            if (vm.AvatarFile is { Length: > 0 })
            {
                if (!IsImage(vm.AvatarFile))
                    ModelState.AddModelError(nameof(vm.AvatarFile), "jpg, png, webp veya gif (≤ 6 MB) yükleyin.");
                else
                {
                    var path = await SaveImage(vm.AvatarFile, me.Id, "avatars");
                    DeleteIfExists(me.AvatarPath);
                    me.AvatarPath = path;
                }
            }

            // Kapak
            if (vm.CoverFile is { Length: > 0 })
            {
                if (!IsImage(vm.CoverFile))
                    ModelState.AddModelError(nameof(vm.CoverFile), "jpg, png, webp veya gif (≤ 6 MB) yükleyin.");
                else
                {
                    var path = await SaveImage(vm.CoverFile, me.Id, "covers");
                    DeleteIfExists(me.CoverPath);
                    me.CoverPath = path;
                }
            }

            if (!ModelState.IsValid)
            {
                vm.AvatarPath = me.AvatarPath;
                vm.CoverPath = me.CoverPath;
                return View(vm);
            }

            await _userManager.UpdateAsync(me);
            TempData["ok"] = "Profil güncellendi.";
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
    }
}
