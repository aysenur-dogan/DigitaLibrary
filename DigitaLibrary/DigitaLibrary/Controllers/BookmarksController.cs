using DigitaLibrary.Data;
using DigitaLibrary.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitaLibrary.Controllers
{
    [Authorize]
    public class BookmarksController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<Admin> _userManager;

        public BookmarksController(AppDbContext db, UserManager<Admin> userManager)
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

            var list = await _db.Bookmarks
                .Where(b => b.UserId == me.Id)
                 .Include(x => x.Work)
        .ThenInclude(w => w.Author)
                .Include(b => b.Work)!.ThenInclude(w => w.Category)
                .OrderByDescending(b => b.Id)
                .ToListAsync();

            return View(list); // Views/Bookmarks/Index.cshtml
        }

        // Ekle/çıkar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int workId)
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return Unauthorized();

            var existing = await _db.Bookmarks
                .FirstOrDefaultAsync(b => b.UserId == me.Id && b.AcademicWorkId == workId);

            if (existing == null)
                _db.Bookmarks.Add(new Bookmark { UserId = me.Id, AcademicWorkId = workId });
            else
                _db.Bookmarks.Remove(existing);

            await _db.SaveChangesAsync();

            return Redirect(Request.Headers["Referer"].ToString() ?? Url.Action("Index", "Home")!);
        }
    }
}
