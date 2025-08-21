using DigitaLibrary.Data;
using DigitaLibrary.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitaLibrary.Controllers
{
    [Authorize]
    public class UserRatingsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<Admin> _userManager;

        public UserRatingsController(AppDbContext db, UserManager<Admin> userManager)
        { _db = db; _userManager = userManager; }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(string ratedUserId, byte score, string? comment)
        {
            var me = await _userManager.GetUserAsync(User);
            if (me == null) return Forbid();

            if (string.IsNullOrWhiteSpace(ratedUserId) || score < 1 || score > 5)
                return BadRequest();

            if (ratedUserId == me.Id)
            {
                TempData["Error"] = "Kendinizi oylayamazsınız.";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            var existing = await _db.UserRatings
                .FirstOrDefaultAsync(x => x.RaterId == me.Id && x.RatedUserId == ratedUserId);

            if (existing == null)
            {
                _db.UserRatings.Add(new UserRating
                {
                    RaterId = me.Id,
                    RatedUserId = ratedUserId,
                    Score = score,
                    Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim()
                });
            }
            else
            {
                existing.Score = score;
                existing.Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
                existing.UpdatedAt = DateTime.UtcNow;
                _db.UserRatings.Update(existing);
            }

            await _db.SaveChangesAsync();
            TempData["Ok"] = "Oyunuz kaydedildi.";
            return Redirect(Request.Headers["Referer"].ToString());
        }

        // İsteğe bağlı: bir kullanıcının aldığı yorumları listele
        [AllowAnonymous]
        public async Task<IActionResult> List(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return NotFound();
            var reviews = await _db.UserRatings
                .Include(x => x.Rater)
                .Where(x => x.RatedUserId == userId)
                .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                .ToListAsync();
            return View(reviews);
        }
    }
}
