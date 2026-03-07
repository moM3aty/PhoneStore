using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PhoneStore.Models.ViewModels
{
    public class ProductViewModel
    {
        public Product Product { get; set; } = new Product();

        [Display(Name = "صورة المنتج")]
        public IFormFile? ImageFile { get; set; }

        public IEnumerable<SelectListItem> CompanyList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> CategoryList { get; set; } = new List<SelectListItem>();


        [Display(Name = "الألوان المتاحة (افصل بينها بفاصلة ،)")]
        public string? ColorsInput { get; set; }

        [Display(Name = "الأنواع/الموديلات المتاحة (افصل بينها بفاصلة ،)")]
        public string? TypesInput { get; set; } 
    }
}