using DigitaLibrary.Data;
using DigitaLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;

namespace DigitaLibrary.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<Admin> _userManager;

        public HomeController(AppDbContext db, ILogger<HomeController> logger, UserManager<Admin> userManager)
        {
            _db = db;
            _logger = logger;
            _userManager = userManager;
        }

        // Ana Sayfa (kategori + arama + favori/kaydet desteði)
        public async Task<IActionResult> Index(int? categoryId, string? q)
        {
            var categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Categories = categories;

            var works = _db.AcademicWorks
                           .Include(a => a.Category)
                           .Include(a => a.Author)
                           .AsQueryable();

            if (categoryId.HasValue)
                works = works.Where(w => w.CategoryId == categoryId);

            if (!string.IsNullOrWhiteSpace(q))
                works = works.Where(w =>
                    w.Title.Contains(q) ||
                    (w.Authors != null && w.Authors.Contains(q)) ||
                    (w.Keywords != null && w.Keywords.Contains(q)));

            var list = await works
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();

            // --- Beðeni & Kaydet bilgileri ekle (KULLANICININ FAVORÝ/KAYIT ID'LERÝ) ---
            if (User.Identity?.IsAuthenticated == true)
            {
                var me = await _userManager.GetUserAsync(User);
                if (me != null)
                {
                    // Favorite -> AcademicWorkId
                    var favIds = await _db.Favorites
                        .Where(f => f.UserId == me.Id)
                        .Select(f => f.AcademicWorkId)
                        .ToListAsync();

                    // Bookmark -> AcademicWorkId
                    var bmIds = await _db.Bookmarks
                        .Where(b => b.UserId == me.Id)
                        .Select(b => b.AcademicWorkId)
                        .ToListAsync();

                    ViewBag.FavIds = favIds.ToHashSet();
                    ViewBag.BmIds = bmIds.ToHashSet();
                }
                else
                {
                    ViewBag.FavIds = new HashSet<int>();
                    ViewBag.BmIds = new HashSet<int>();
                }
            }
            else
            {
                ViewBag.FavIds = new HashSet<int>();
                ViewBag.BmIds = new HashSet<int>();
            }

            return View(list);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
