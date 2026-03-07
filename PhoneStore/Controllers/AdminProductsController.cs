using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PhoneStore.Data;
using PhoneStore.Filters;
using PhoneStore.Models;
using PhoneStore.Models.ViewModels;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace PhoneStore.Controllers
{
    [ServiceFilter(typeof(AdminAuthFilter))]
    public class AdminProductsController : Controller
    {
        private readonly StoreDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminProductsController(StoreDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // تم إضافة معامل page وإعدادات التصفح
        public async Task<IActionResult> Index(string searchString, int? companyId, int page = 1)
        {
            int pageSize = 30; // عدد المنتجات في كل صفحة

            var productsQuery = _context.Products
                                   .Include(p => p.Company)
                                   .Include(p => p.Category)
                                   .AsQueryable();

            // الفلترة
            if (!string.IsNullOrEmpty(searchString))
            {
                productsQuery = productsQuery.Where(s => s.Name.Contains(searchString));
            }

            if (companyId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CompanyId == companyId);
            }

            // --- حسابات التصفح (Pagination) ---
            int totalItems = await productsQuery.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // التأكد من أن الصفحة الحالية ضمن النطاق
            page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));

            var products = await productsQuery
                                .OrderByDescending(p => p.Id)
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();

            // تمرير البيانات للواجهة
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", companyId);
            ViewData["CurrentFilter"] = searchString;
            ViewData["SelectedCompany"] = companyId;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new ProductViewModel
            {
                CompanyList = await GetCompanySelectListAsync(),
                CategoryList = await GetCategorySelectListAsync()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (viewModel.ImageFile != null)
                {
                    viewModel.Product.ImageUrl = await SaveImageAsync(viewModel.ImageFile);
                }

                if (!string.IsNullOrEmpty(viewModel.ColorsInput))
                {
                    var colors = viewModel.ColorsInput.Split(new[] { ',', '،' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var color in colors)
                    {
                        viewModel.Product.Colors.Add(new ProductColor { ColorName = color.Trim() });
                    }
                }

                if (!string.IsNullOrEmpty(viewModel.TypesInput))
                {
                    var types = viewModel.TypesInput.Split(new[] { ',', '،' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var type in types)
                    {
                        viewModel.Product.Types.Add(new ProductType { TypeName = type.Trim() });
                    }
                }

                _context.Add(viewModel.Product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            viewModel.CompanyList = await GetCompanySelectListAsync(viewModel.Product.CompanyId);
            viewModel.CategoryList = await GetCategorySelectListAsync(viewModel.Product.CategoryId);
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                                        .Include(p => p.Colors)
                                        .Include(p => p.Types)
                                        .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            var viewModel = new ProductViewModel
            {
                Product = product,
                CompanyList = await GetCompanySelectListAsync(product.CompanyId),
                CategoryList = await GetCategorySelectListAsync(product.CategoryId),
                ColorsInput = string.Join(", ", product.Colors.Select(c => c.ColorName)),
                TypesInput = string.Join(", ", product.Types.Select(t => t.TypeName))
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductViewModel viewModel)
        {
            if (id != viewModel.Product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var productFromDb = await _context.Products
                                                      .Include(p => p.Colors)
                                                      .Include(p => p.Types)
                                                      .FirstOrDefaultAsync(p => p.Id == id);

                    if (productFromDb == null) return NotFound();

                    productFromDb.Name = viewModel.Product.Name;
                    productFromDb.Description = viewModel.Product.Description;
                    productFromDb.Price = viewModel.Product.Price;
                    productFromDb.OldPrice = viewModel.Product.OldPrice;
                    productFromDb.CategoryId = viewModel.Product.CategoryId;
                    productFromDb.CompanyId = viewModel.Product.CompanyId;

                    if (viewModel.ImageFile != null)
                    {
                        DeleteImage(productFromDb.ImageUrl);
                        productFromDb.ImageUrl = await SaveImageAsync(viewModel.ImageFile);
                    }

                    productFromDb.Colors.Clear();
                    if (!string.IsNullOrEmpty(viewModel.ColorsInput))
                    {
                        var colors = viewModel.ColorsInput.Split(new[] { ',', '،' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var color in colors)
                        {
                            productFromDb.Colors.Add(new ProductColor { ColorName = color.Trim() });
                        }
                    }

                    productFromDb.Types.Clear();
                    if (!string.IsNullOrEmpty(viewModel.TypesInput))
                    {
                        var types = viewModel.TypesInput.Split(new[] { ',', '،' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var type in types)
                        {
                            productFromDb.Types.Add(new ProductType { TypeName = type.Trim() });
                        }
                    }

                    _context.Update(productFromDb);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.Id == viewModel.Product.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            viewModel.CompanyList = await GetCompanySelectListAsync(viewModel.Product.CompanyId);
            viewModel.CategoryList = await GetCategorySelectListAsync(viewModel.Product.CategoryId);
            return View(viewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products
                .Include(p => p.Company)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                DeleteImage(product.ImageUrl);
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string productsPath = Path.Combine(wwwRootPath, "images", "products");
            if (!Directory.Exists(productsPath)) Directory.CreateDirectory(productsPath);
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            string filePath = Path.Combine(productsPath, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create)) await imageFile.CopyToAsync(fileStream);
            return "/images/products/" + fileName;
        }

        private void DeleteImage(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string imagePath = Path.Combine(wwwRootPath, imageUrl.TrimStart('/'));
            if (System.IO.File.Exists(imagePath)) System.IO.File.Delete(imagePath);
        }

        private async Task<IEnumerable<SelectListItem>> GetCompanySelectListAsync(int? selectedId = null)
        {
            return await _context.Companies.OrderBy(c => c.Name)
               .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name, Selected = selectedId.HasValue && c.Id == selectedId.Value })
               .ToListAsync();
        }

        private async Task<IEnumerable<SelectListItem>> GetCategorySelectListAsync(int? selectedId = null)
        {
            return await _context.Categories.OrderBy(c => c.Name)
               .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name, Selected = selectedId.HasValue && c.Id == selectedId.Value })
               .ToListAsync();
        }
    }
}