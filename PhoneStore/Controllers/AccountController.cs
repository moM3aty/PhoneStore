using Microsoft.AspNetCore.Mvc;
using PhoneStore.Models.ViewModels;

namespace PhoneStore.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;

        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model, string? returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                string adminPassword = _configuration.GetValue<string>("AdminPassword") ?? "supersecret";

                if (model.Password == adminPassword)
                {
                    HttpContext.Session.SetString("IsAdmin", "true");

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                }

                ModelState.AddModelError("", "كلمة المرور غير صحيحة.");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("IsAdmin");
            return RedirectToAction("Index", "Home");
        }
    }
}