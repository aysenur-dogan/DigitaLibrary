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

            var posts = await _db.Set<Post>()
                                 .AsNoTracking()
                                 .Where(p => p.AuthorId == me.Id)
                                 .OrderByDescending(p => p.CreatedAt)
                                 .ToListAsync();

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
                MyWorks = myWorks,
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
                .Where(x => x.AuthorId == id)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            var vm = new ProfilePageViewModel
            {
                User = user,
                MyWorks = works,
                TotalPosts = 0,
                PublishedPosts = 0,
                DraftPosts = 0,
                IsOwner = (viewer?.Id == id)        // 👈 SAHİBİ Mİ?
            };

            return View("Me", vm);                  // 👈 Aynı, şık profil sayfası
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

            var display = $"{vm.FirstName} {vm.LastName}".Trim();
            me.DisplayName = string.IsNullOrWhiteSpace(display) ? vm.FirstName?.Trim() : display;

            me.Bio = vm.Bio?.Trim();
            me.EducationInfo = vm.EducationInfo?.Trim();
            me.Interests = vm.Interests?.Trim();

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

            if (!string.IsNullOrWhiteSpace(vm.Email) && vm.Email != me.Email)
            {
                var setEmail = await _userManager.SetEmailAsync(me, vm.Email.Trim());
                if (!setEmail.Succeeded)
                {
                    foreach (var e in setEmail.Errors)
                        ModelState.AddModelError(nameof(vm.Email), e.Description);
                    vm.AvatarPath = me.AvatarPath;
                    vm.CoverPath = me.CoverPath;
                    // ⭐ RATING: kendi profilim için ortalama ve oy sayısı (form göstermeyeceğiz)
                    var myAvg = await _db.UserRatings
                        .Where(r => r.RatedUserId == me.Id)
                        .AverageAsync(r => (double?)r.Score) ?? 0;

                    var myCnt = await _db.UserRatings
                        .CountAsync(r => r.RatedUserId == me.Id);

                    ViewBag.RatingAvg = Math.Round(myAvg, 2);
                    ViewBag.RatingCount = myCnt;

                    return View(vm);
                }
            }

            if (vm.AvatarFile is { Length: > 0 })
            {
                var path = await SaveImage(vm.AvatarFile, me.Id, "avatars");
                DeleteIfExists(me.AvatarPath);
                me.AvatarPath = path;
            }

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
