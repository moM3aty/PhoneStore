using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneStore.Data;
using PhoneStore.Filters;
using PhoneStore.Models;
using PhoneStore.Models.ViewModels;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace PhoneStore.Controllers
{
    [ServiceFilter(typeof(AdminAuthFilter))]
    public class AdminCompaniesController : Controller
    {
        private readonly StoreDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminCompaniesController(StoreDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var companies = _context.Companies.Include(c => c.Products).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                companies = companies.Where(s => s.Name.Contains(searchString));
            }

            ViewData["CurrentFilter"] = searchString;

            return View(await companies.OrderByDescending(c => c.Id).ToListAsync());
        }

        public IActionResult Create()
        {
            return View(new CompanyViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CompanyViewModel viewModel)
        {
            ModelState.Remove("Company.ImageUrl");

            if (ModelState.IsValid)
            {
                if (viewModel.ImageFile != null)
                {
                    viewModel.Company.ImageUrl = await SaveImageAsync(viewModel.ImageFile);
                }

                _context.Add(viewModel.Company);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var company = await _context.Companies.FindAsync(id);
            if (company == null) return NotFound();

            var viewModel = new CompanyViewModel
            {
                Company = company
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CompanyViewModel viewModel)
        {
            if (id != viewModel.Company.Id) return NotFound();

            ModelState.Remove("Company.ImageUrl");

            if (ModelState.IsValid)
            {
                try
                {
                    var companyFromDb = await _context.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
                    if (companyFromDb == null) return NotFound();

                    if (viewModel.ImageFile != null)
                    {
                        DeleteImage(companyFromDb.ImageUrl);
                        viewModel.Company.ImageUrl = await SaveImageAsync(viewModel.ImageFile);
                    }
                    else
                    {
                        viewModel.Company.ImageUrl = companyFromDb.ImageUrl;
                    }

                    _context.Update(viewModel.Company);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Companies.Any(e => e.Id == viewModel.Company.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null) return NotFound();

            return View(company);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                DeleteImage(company.ImageUrl);
                _context.Companies.Remove(company);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string companiesPath = Path.Combine(wwwRootPath, "images", "companies");

            if (!Directory.Exists(companiesPath))
            {
                Directory.CreateDirectory(companiesPath);
            }

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            string filePath = Path.Combine(companiesPath, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return "/images/companies/" + fileName;
        }

        private void DeleteImage(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string imagePath = Path.Combine(wwwRootPath, imageUrl.TrimStart('/'));

            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
        }
    }
}