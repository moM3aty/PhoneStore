using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneStore.Data;
using PhoneStore.Filters;
using PhoneStore.Models;
using PhoneStore.Models.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using PhoneStore.Extensions; 

namespace PhoneStore.Controllers
{
    [ServiceFilter(typeof(AdminAuthFilter))]
    public class AdminController : Controller
    {
        private readonly StoreDbContext _context;

        public AdminController(StoreDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel
            {
                TotalProducts = await _context.Products.CountAsync(),
                TotalOrders = await _context.Orders.CountAsync(),
                TotalCompanies = await _context.Companies.CountAsync(),
                TotalRevenue = await _context.Orders.Where(o => o.Status != OrderStatus.Cancelled).SumAsync(o => o.TotalAmount)
            };


            var ordersByStatus = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var item in ordersByStatus)
            {
                viewModel.OrderStatusLabels.Add(item.Status.GetDisplayName());
                viewModel.OrderStatusData.Add(item.Count);
            }

            var productsByCompany = await _context.Companies
                .Select(c => new { c.Name, ProductCount = c.Products.Count })
                .OrderByDescending(x => x.ProductCount)
                .Take(10) 
                .ToListAsync();

            foreach (var item in productsByCompany)
            {
                viewModel.CompanyLabels.Add(item.Name);
                viewModel.CompanyProductCounts.Add(item.ProductCount);
            }

            return View(viewModel);
        }
    }
}