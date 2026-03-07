using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneStore.Data;
using PhoneStore.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System;

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

        // ⚠️ تأكد من وجود علامة الاستفهام بجانب int هنا أيضاً
        public async Task<IActionResult> Index(int? companyId, int? categoryId, string searchString, int page = 1)
        {
            int pageSize = 30; // عدد المنتجات في كل صفحة

            var productsQuery = _context.Products
                                        .Include(p => p.Company)
                                        .Include(p => p.Category)
                                        .AsQueryable();

            // الفلترة بالبحث النصي
            if (!string.IsNullOrEmpty(searchString))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(searchString) ||
                                                         (p.Description != null && p.Description.Contains(searchString)));
            }

            // الفلترة بالشركة
            if (companyId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CompanyId == companyId.Value);
            }

            // الفلترة بالقسم (الربط الذكي للقسم الرئيسي والفرعي)
            if (categoryId.HasValue)
            {
                var selectedCategory = await _context.Categories
                                                     .Include(c => c.SubCategories)
                                                     .FirstOrDefaultAsync(c => c.Id == categoryId.Value);

                if (selectedCategory != null)
                {
                    if (selectedCategory.SubCategories != null && selectedCategory.SubCategories.Any())
                    {
                        var subCategoryIds = selectedCategory.SubCategories.Select(s => s.Id).ToList();
                        subCategoryIds.Add(selectedCategory.Id);

                        productsQuery = productsQuery.Where(p => p.CategoryId > 0 && subCategoryIds.Contains(p.CategoryId));
                    }
                    else
                    {
                        productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
                    }
                }
            }

            // حساب التصفح
            int totalItems = await productsQuery.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));

            var products = await productsQuery
                                .OrderByDescending(p => p.Id)
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();

            var companies = await _context.Companies.ToListAsync();
            var categories = await _context.Categories.ToListAsync();
            var wishlistIds = GetWishlistFromCookie();

            var viewModel = new StoreViewModel
            {
                Companies = companies,
                Products = products,
                Categories = categories,
                SelectedCompanyId = companyId,
                SelectedCategoryId = categoryId,
                SearchString = searchString,
                WishlistIds = wishlistIds,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Company)
                .Include(p => p.Category)
                .Include(p => p.Colors)
                .Include(p => p.Types)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            var wishlistIds = GetWishlistFromCookie();
            ViewBag.InWishlist = wishlistIds.Contains(product.Id);

            return View(product);
        }
    }
}