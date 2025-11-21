using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneStore.Data;
using PhoneStore.Filters;
using PhoneStore.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PhoneStore.Controllers
{
    [ServiceFilter(typeof(AdminAuthFilter))]
    public class AdminAnnouncementsController : Controller
    {
        private readonly StoreDbContext _context;

        public AdminAnnouncementsController(StoreDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Announcements.OrderByDescending(a => a.Id).ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Message,IsActive")] Announcement announcement)
        {
            if (ModelState.IsValid)
            {
                if (announcement.IsActive)
                {
                    var activeAnnouncements = await _context.Announcements.Where(a => a.IsActive).ToListAsync();
                    foreach (var item in activeAnnouncements)
                    {
                        item.IsActive = false;
                    }
                }

                _context.Add(announcement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(announcement);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null) return NotFound();
            return View(announcement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Message,IsActive,CreatedAt")] Announcement announcement)
        {
            if (id != announcement.Id) return NotFound();

            if (ModelState.IsValid)
            {
                if (announcement.IsActive)
                {
                    var otherActive = await _context.Announcements.Where(a => a.IsActive && a.Id != id).ToListAsync();
                    foreach (var item in otherActive) item.IsActive = false;
                }

                _context.Update(announcement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(announcement);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var announcement = await _context.Announcements.FirstOrDefaultAsync(m => m.Id == id);
            if (announcement == null) return NotFound();
            return View(announcement);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement != null)
            {
                _context.Announcements.Remove(announcement);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ToggleStatus(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement != null)
            {
                bool newState = !announcement.IsActive;

                if (newState)
                {
                    var others = await _context.Announcements.Where(a => a.IsActive).ToListAsync();
                    foreach (var o in others) o.IsActive = false;
                }

                announcement.IsActive = newState;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}