using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneStore.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PhoneStore.ViewComponents
{
    public class AnnouncementViewComponent : ViewComponent
    {
        private readonly StoreDbContext _context;

        public AnnouncementViewComponent(StoreDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var activeAnnouncement = await _context.Announcements
                                           .Where(a => a.IsActive)
                                           .OrderByDescending(a => a.Id)
                                           .FirstOrDefaultAsync();

            return View(activeAnnouncement);
        }
    }
}