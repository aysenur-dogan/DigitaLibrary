using System.Linq;
using System.Threading.Tasks;
using DigitaLibrary.Data;              // DbContext namespace'in
using DigitaLibrary.Models;            // Admin, Post
using DigitaLibrary.ViewModels;        // ProfilePageViewModel
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitaLibrary.Controllers
{
    [Authorize] // sadece giriş yapanlar
    public class ProfileController : Controller
    {
        private readonly UserManager<Admin> _userManager;
        private readonly AppDbContext _db; // DbContext adın AppDbContext ise

        public ProfileController(UserManager<Admin> userManager, AppDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public async Task<IActionResult> Me()
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return Challenge();

            var posts = await _db.Set<Post>()
                                 .Where(p => p.AuthorId == me.Id)
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
    }
}
