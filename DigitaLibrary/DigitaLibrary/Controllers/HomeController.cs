using DigitaLibrary.Data;
using DigitaLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace DigitaLibrary.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ILogger<HomeController> _logger;

        public HomeController(AppDbContext db, ILogger<HomeController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // Ana Sayfa (kategori + arama desteði)
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

            var list = await works.OrderByDescending(w => w.CreatedAt).ToListAsync();
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
