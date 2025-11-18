using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace PhoneStore.Models.ViewModels
{
    public class CompanyViewModel
    {
        public Company Company { get; set; } = new Company();

        [Display(Name = "شعار الشركة")]
        public IFormFile? ImageFile { get; set; }
    }
}