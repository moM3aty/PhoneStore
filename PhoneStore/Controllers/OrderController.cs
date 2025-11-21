using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneStore.Data;
using PhoneStore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json; 

namespace PhoneStore.Controllers
{
    public class OrderController : Controller
    {
        private readonly StoreDbContext _context;

        public OrderController(StoreDbContext context)
        {
            _context = context;
        }

        private List<int> GetOrderIdsFromCookie()
        {
            var cookie = Request.Cookies["OrderHistory"];
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

        public async Task<IActionResult> Index()
        {
            var orderHistoryIds = GetOrderIdsFromCookie();

            if (orderHistoryIds.Count == 0)
            {
                return View(new List<Order>());
            }

            var orders = await _context.Orders
                                     .Include(o => o.DeliveryLocation)
                                     .Where(o => orderHistoryIds.Contains(o.Id))
                                     .OrderByDescending(o => o.OrderDate)
                                     .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return RedirectToAction("Index");

            var orderHistoryIds = GetOrderIdsFromCookie();

            if (!orderHistoryIds.Contains(id.Value))
            {
                return RedirectToAction("Index");
            }

            var order = await _context.Orders
                                    .Include(o => o.DeliveryLocation)
                                    .Include(o => o.OrderDetails)
                                        .ThenInclude(od => od.Product)
                                            .ThenInclude(p => p.Company)
                                    .Include(o => o.OrderDetails)
                                        .ThenInclude(od => od.Product)
                                            .ThenInclude(p => p.Category)
                                    .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return RedirectToAction("Index");

            return View(order);
        }
    }
}