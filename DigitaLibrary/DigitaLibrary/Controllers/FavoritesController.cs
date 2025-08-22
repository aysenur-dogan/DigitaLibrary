using DigitaLibrary.Data;
using DigitaLibrary.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitaLibrary.Controllers
{
    [Authorize]
    public class FavoritesController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<Admin> _userManager;

        public FavoritesController(AppDbContext db, UserManager<Admin> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // Liste
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return Challenge();

            var list = await _db.Favorites
                .Where(f => f.UserId == me.Id)
                .Include(f => f.Work)!
                    .ThenInclude(w => w.Author)   // 👈 yazar bilgisini dahil et
                .Include(f => f.Work)!
                    .ThenInclude(w => w.Category)
                .OrderByDescending(f => f.Id)
                .ToListAsync();

            return View(list); // Views/Favorites/Index.cshtml
        }

        // Ekle/çıkar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int workId)
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return Unauthorized();

            var existing = await _db.Favorites
                .FirstOrDefaultAsync(f => f.UserId == me.Id && f.AcademicWorkId == workId);

            if (existing == null)
            {
                _db.Favorites.Add(new Favorite { UserId = me.Id, AcademicWorkId = workId });
            }
            else
            {
                _db.Favorites.Remove(existing);
            }

            await _db.SaveChangesAsync();

            // Güvenli geri dönüş
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrWhiteSpace(referer))
                return Redirect(referer);

            return RedirectToAction("Index", "Home");
        }
    }
}
