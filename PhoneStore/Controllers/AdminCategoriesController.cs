using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PhoneStore.Data;
using PhoneStore.Filters;
using PhoneStore.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PhoneStore.Controllers
{
    [ServiceFilter(typeof(AdminAuthFilter))]
    public class AdminCategoriesController : Controller
    {
        private readonly StoreDbContext _context;

        public AdminCategoriesController(StoreDbContext context)
        {
            _context = context;
        }

        // عرض الأقسام مع قسمها الرئيسي (إن وجد)
        public async Task<IActionResult> Index(string searchString)
        {
            var categories = _context.Categories
                                     .Include(c => c.Products)
                                     .Include(c => c.ParentCategory) // جلب القسم الأب
                                     .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                categories = categories.Where(s => s.Name.Contains(searchString));
            }

            ViewData["CurrentFilter"] = searchString;

            return View(await categories.OrderByDescending(c => c.Id).ToListAsync());
        }

        public IActionResult Create()
        {
            // إرسال قائمة الأقسام للـ DropDown
            ViewData["ParentCategories"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ParentCategoryId")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ParentCategories"] = new SelectList(_context.Categories, "Id", "Name", category.ParentCategoryId);
            return View(category);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            // إرسال الأقسام، مع استبعاد القسم الحالي حتى لا يختار نفسه كقسم أب!
            var potentialParents = _context.Categories.Where(c => c.Id != id).ToList();
            ViewData["ParentCategories"] = new SelectList(potentialParents, "Id", "Name", category.ParentCategoryId);

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ParentCategoryId")] Category category)
        {
            if (id != category.Id) return NotFound();

            // منع القسم من أن يكون أباً لنفسه كإجراء حماية إضافي
            if (category.ParentCategoryId == category.Id)
            {
                ModelState.AddModelError("ParentCategoryId", "لا يمكن للقسم أن يكون فرعاً من نفسه!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Categories.Any(e => e.Id == category.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            var potentialParents = _context.Categories.Where(c => c.Id != id).ToList();
            ViewData["ParentCategories"] = new SelectList(potentialParents, "Id", "Name", category.ParentCategoryId);
            return View(category);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var category = await _context.Categories
                                         .Include(c => c.ParentCategory)
                                         .Include(c => c.SubCategories) // للتأكد مما إذا كان لديه أقسام فرعية
                                         .Include(c => c.Products)
                                         .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories
                                         .Include(c => c.SubCategories)
                                         .FirstOrDefaultAsync(c => c.Id == id);

            if (category != null)
            {
                // إذا كان لديه أقسام فرعية، لا يمكن حذفه مباشرة (حماية البيانات)
                if (category.SubCategories.Any())
                {
                    TempData["ErrorMessage"] = "لا يمكن حذف هذا القسم لأنه يحتوي على أقسام فرعية. يرجى حذف الأقسام الفرعية أولاً.";
                    return RedirectToAction(nameof(Delete), new { id = id });
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}