using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace PhoneStore.Models.ViewModels
{
    public class AdminOrdersViewModel
    {
        public Order Order { get; set; }
        public IEnumerable<SelectListItem> StatusList { get; set; }
    }
}