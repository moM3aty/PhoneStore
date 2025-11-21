using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PhoneStore.Data;
using PhoneStore.Filters;
using PhoneStore.Models;
using PhoneStore.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System;

namespace PhoneStore.Controllers
{
    [ServiceFilter(typeof(AdminAuthFilter))]
    public class AdminOrdersController : Controller
    {
        private readonly StoreDbContext _context;

        public AdminOrdersController(StoreDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, string status)
        {
            var ordersQuery = _context.Orders
                                      .Include(o => o.DeliveryLocation)
                                      .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                if (int.TryParse(searchString, out int orderId))
                {
                    ordersQuery = ordersQuery.Where(o => o.Id == orderId);
                }
                else
                {
                    ordersQuery = ordersQuery.Where(o => o.CustomerName.Contains(searchString) || o.PhoneNumber.Contains(searchString));
                }
            }

            if (!string.IsNullOrEmpty(status) && Enum.TryParse(typeof(OrderStatus), status, out object statusValue))
            {
                ordersQuery = ordersQuery.Where(o => o.Status == (OrderStatus)statusValue);
            }

            ViewData["CurrentFilter"] = searchString;
            ViewData["SelectedStatus"] = status;

            var orders = await ordersQuery.OrderByDescending(o => o.Id).ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.DeliveryLocation)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Company) 
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Category) 
                        .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null) return NotFound();

            var viewModel = new AdminOrdersViewModel
            {
                Order = order,
                StatusList = Enum.GetValues(typeof(OrderStatus))
                                 .Cast<OrderStatus>()
                                 .Select(s => new SelectListItem
                                 {
                                     Value = s.ToString(),
                                     Text = GetDisplayName(s),
                                     Selected = s == order.Status
                                 })
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = status;
            _context.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = id });
        }

        public async Task<IActionResult> PrintInvoice(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.DeliveryLocation)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Company) 
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Category) 
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders.FirstOrDefaultAsync(m => m.Id == id);
            if (order == null) return NotFound();

            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == id);
            if (order != null)
            {
                _context.OrderDetails.RemoveRange(order.OrderDetails);
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private string GetDisplayName(Enum enumValue)
        {
            var displayAttribute = enumValue.GetType()
                                            .GetMember(enumValue.ToString())
                                            .First()
                                            .GetCustomAttribute<DisplayAttribute>();
            return displayAttribute?.GetName() ?? enumValue.ToString();
        }
    }
}