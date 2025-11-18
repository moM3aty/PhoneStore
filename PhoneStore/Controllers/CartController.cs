using Microsoft.AspNetCore.Mvc;
using PhoneStore.Data;
using PhoneStore.Extensions;
using PhoneStore.Models;
using PhoneStore.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Text.Json;

namespace PhoneStore.Controllers
{
    public class IdOnlyModel
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
    }

    public class CartController : Controller
    {
        private readonly StoreDbContext _context;
        public const string CartSessionKey = "_Cart";
        public const string WishlistCookieKey = "PhoneStore_Wishlist";

        public CartController(StoreDbContext context)
        {
            _context = context;
        }

        // --- دوال مساعدة للتعامل مع كوكيز المفضلة ---
        private List<int> GetWishlistFromCookie()
        {
            var cookie = Request.Cookies[WishlistCookieKey];
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

        private void SaveWishlistToCookie(List<int> wishlist)
        {
            var options = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(30),
                HttpOnly = true,
                IsEssential = true
            };
            Response.Cookies.Append(WishlistCookieKey, JsonSerializer.Serialize(wishlist), options);
        }
        // ------------------------------------------------

        public async Task<IActionResult> Index()
        {
            var cartSession = HttpContext.Session.GetObject<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            var viewModel = await GetCartViewModelAsync(cartSession);
            return View(viewModel);
        }

        public async Task<IActionResult> Wishlist()
        {
            var wishlistIds = GetWishlistFromCookie();
            var products = await _context.Products
                                       .Where(p => wishlistIds.Contains(p.Id))
                                       .ToListAsync();

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cartSession = HttpContext.Session.GetObject<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            if (cartSession.Count == 0) return RedirectToAction("Index");

            var cartViewModel = await GetCartViewModelAsync(cartSession);
            var locations = await _context.DeliveryLocations.OrderBy(l => l.LocationName).ToListAsync();

            var viewModel = new CheckoutViewModel
            {
                CartSummary = cartViewModel,
                DeliveryLocations = locations.Select(l => new SelectListItem
                {
                    Value = l.Id.ToString(),
                    Text = $"{l.LocationName} (+{l.DeliveryFee.ToString("C", new System.Globalization.CultureInfo("ar-JO"))})"
                })
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel viewModel)
        {
            var cartSession = HttpContext.Session.GetObject<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            var cartViewModel = await GetCartViewModelAsync(cartSession);

            if (cartSession.Count == 0) ModelState.AddModelError("", "سلة التسوق فارغة!");
            ModelState.Remove("CartSummary");
            ModelState.Remove("DeliveryLocations");
            if (ModelState.IsValid)
            {
                var deliveryLocation = await _context.DeliveryLocations.FindAsync(viewModel.DeliveryLocationId);
                if (deliveryLocation == null)
                {
                    ModelState.AddModelError("DeliveryLocationId", "منطقة التوصيل غير صالحة.");
                }
                else
                {
                    var order = new Order
                    {
                        CustomerName = viewModel.CustomerName,
                        PhoneNumber = viewModel.PhoneNumber,
                        Address = viewModel.Address,
                        DeliveryLocationId = viewModel.DeliveryLocationId,
                        PaymentMethod = viewModel.PaymentMethod,
                        OrderDate = DateTime.Now,
                        Status = OrderStatus.Pending,
                        TotalAmount = cartViewModel.TotalAmount + deliveryLocation.DeliveryFee
                    };

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    foreach (var item in cartViewModel.CartItems)
                    {
                        var orderDetail = new OrderDetail
                        {
                            OrderId = order.Id,
                            ProductId = item.Product.Id,
                            Quantity = item.Quantity,
                            UnitPrice = item.Product.Price
                        };
                        _context.OrderDetails.Add(orderDetail);
                    }
                    await _context.SaveChangesAsync();

                    HttpContext.Session.Remove(CartSessionKey);

                    var orderHistoryIds = new List<int>();
                    var cookie = Request.Cookies["OrderHistory"];
                    if (!string.IsNullOrEmpty(cookie))
                    {
                        try { orderHistoryIds = JsonSerializer.Deserialize<List<int>>(cookie) ?? new List<int>(); } catch { }
                    }

                    if (!orderHistoryIds.Contains(order.Id))
                    {
                        orderHistoryIds.Add(order.Id);
                        var cookieOptions = new CookieOptions
                        {
                            Expires = DateTime.Now.AddDays(90),
                            HttpOnly = true,
                            IsEssential = true
                        };
                        Response.Cookies.Append("OrderHistory", JsonSerializer.Serialize(orderHistoryIds), cookieOptions);
                    }

                    return RedirectToAction("OrderConfirmation", new { id = order.Id });
                }
            }

            viewModel.CartSummary = cartViewModel;
            var locations = await _context.DeliveryLocations.OrderBy(l => l.LocationName).ToListAsync();
            viewModel.DeliveryLocations = locations.Select(l => new SelectListItem
            {
                Value = l.Id.ToString(),
                Text = $"{l.LocationName} (+{l.DeliveryFee.ToString("C", new System.Globalization.CultureInfo("ar-JO"))})"
            });

            return View(viewModel);
        }

        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var order = await _context.Orders.Include(o => o.DeliveryLocation).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return RedirectToAction("Index", "Home");
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] IdOnlyModel model)
        {
            var product = await _context.Products.FindAsync(model.Id);
            if (product == null) return NotFound();

            var cart = HttpContext.Session.GetObject<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            var existingItem = cart.FirstOrDefault(item => item.ProductId == model.Id);

            if (existingItem != null) existingItem.Quantity += 1;
            else cart.Add(new CartItem { ProductId = model.Id, Quantity = 1 });

            HttpContext.Session.SetObject(CartSessionKey, cart);
            return Ok(new { success = true, message = $"تمت إضافة {product.Name} إلى السلة!", count = cart.Sum(c => c.Quantity) });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            var cartItem = cart.FirstOrDefault(item => item.ProductId == id);

            if (cartItem != null && quantity > 0) cartItem.Quantity = quantity;
            else if (cartItem != null && quantity <= 0) cart.Remove(cartItem);

            HttpContext.Session.SetObject(CartSessionKey, cart);
            var viewModel = await GetCartViewModelAsync(cart);
            return PartialView("Index", viewModel);
        }

        [HttpPost]
        public IActionResult RemoveFromCart([FromBody] IdOnlyModel model)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            var cartItem = cart.FirstOrDefault(item => item.ProductId == model.Id);
            if (cartItem != null)
            {
                cart.Remove(cartItem);
                HttpContext.Session.SetObject(CartSessionKey, cart);
            }
            return Ok(new { success = true, message = "تم حذف المنتج من السلة!", count = cart.Sum(c => c.Quantity) });
        }

        [HttpPost]
        public async Task<IActionResult> AddToWishlist([FromBody] IdOnlyModel model)
        {
            var wishlist = GetWishlistFromCookie();
            var product = await _context.Products.FindAsync(model.Id);

            if (product == null) return NotFound();

            if (!wishlist.Contains(model.Id))
            {
                wishlist.Add(model.Id);
                SaveWishlistToCookie(wishlist);
                return Ok(new { success = true, message = $"تم إضافة {product.Name} إلى المفضلة!", count = wishlist.Count });
            }
            return Ok(new { success = false, message = "المنتج موجود بالفعل في المفضلة.", count = wishlist.Count });
        }

        [HttpPost]
        public IActionResult RemoveFromWishlist([FromBody] IdOnlyModel model)
        {
            var wishlist = GetWishlistFromCookie();
            if (wishlist.Remove(model.Id))
            {
                SaveWishlistToCookie(wishlist);
                return Ok(new { success = true, message = "تم حذف المنتج من المفضلة!", count = wishlist.Count });
            }
            return Ok(new { success = false, message = "المنتج غير موجود في المفضلة.", count = wishlist.Count });
        }

        [HttpPost]
        public async Task<IActionResult> GetDeliveryFee(int id)
        {
            var location = await _context.DeliveryLocations.FindAsync(id);
            if (location == null) return Json(new { success = false });
            return Json(new { success = true, fee = location.DeliveryFee });
        }

        private async Task<CartViewModel> GetCartViewModelAsync(List<CartItem> cartSession)
        {
            var viewModel = new CartViewModel();
            foreach (var item in cartSession)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    viewModel.CartItems.Add(new CartItemViewModel { Product = product, Quantity = item.Quantity });
                }
            }
            viewModel.TotalAmount = viewModel.CartItems.Sum(item => item.Subtotal);
            return viewModel;
        }
    }

}