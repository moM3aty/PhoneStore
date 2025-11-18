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
    public class AdminLocationsController : Controller
    {
        private readonly StoreDbContext _context;

        public AdminLocationsController(StoreDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.DeliveryLocations.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,LocationName,DeliveryFee")] DeliveryLocation deliveryLocation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(deliveryLocation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // تم حذف المفتاح
            }
            return View(deliveryLocation);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var location = await _context.DeliveryLocations.FindAsync(id);
            if (location == null) return NotFound();
            return View(location);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,LocationName,DeliveryFee")] DeliveryLocation deliveryLocation)
        {
            if (id != deliveryLocation.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(deliveryLocation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.DeliveryLocations.Any(e => e.Id == deliveryLocation.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index)); // تم حذف المفتاح
            }
            return View(deliveryLocation);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var location = await _context.DeliveryLocations.FirstOrDefaultAsync(m => m.Id == id);
            if (location == null) return NotFound();
            return View(location);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var location = await _context.DeliveryLocations.FindAsync(id);
            if (location != null)
            {
                _context.DeliveryLocations.Remove(location);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index)); // تم حذف المفتاح
        }

        // تم حذف دالة GetAdminKey()
    }
}