using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace PhoneStore.Filters
{
    public class AdminAuthFilter : IAsyncAuthorizationFilter
    {
        public AdminAuthFilter()
        {
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context.HttpContext.Session.GetString("IsAdmin") != "true")
            {
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;

                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl = returnUrl });
                return;
            }

            await Task.CompletedTask;
        }
    }
}