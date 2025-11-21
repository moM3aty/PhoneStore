using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneStore.Data;
using PhoneStore.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;

namespace PhoneStore.Controllers
{
    public class StoreController : Controller
    {
        private readonly StoreDbContext _context;

        public StoreController(StoreDbContext context)
        {
            _context = context;
        }

        private List<int> GetWishlistFromCookie()
        {
            var cookie = Request.Cookies["PhoneStore_Wishlist"];
            if (string.IsNullOrEmpty(cookie)) return new List<int>();
            try
            {
                return JsonSerializer.Deserialize<List<int>>(cookie) ?? new List<int>();
            }
            catch
            {
                return new List<int>();
            }
        }

        public async Task<IActionResult> Index(int? companyId, int? categoryId)
        {
            var productsQuery = _context.Products
                                        .Include(p => p.Company)
                                        .Include(p => p.Category)
                                        .AsQueryable();

            if (companyId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CompanyId == companyId.Value);
            }

            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
            }

            var companies = await _context.Companies.ToListAsync();
            var categories = await _context.Categories.ToListAsync();
            var products = await productsQuery.ToListAsync();

            var wishlistIds = GetWishlistFromCookie();

            var viewModel = new StoreViewModel
            {
                Companies = companies,
                Products = products,
                Categories = categories,
                SelectedCompanyId = companyId,
                SelectedCategoryId = categoryId,
                WishlistIds = wishlistIds 
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Company)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            var wishlistIds = GetWishlistFromCookie();
            ViewBag.InWishlist = wishlistIds.Contains(product.Id);

            return View(product);
        }
    }
}