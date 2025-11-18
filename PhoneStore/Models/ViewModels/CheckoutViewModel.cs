using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PhoneStore.Models.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "اسم العميل مطلوب")]
        [Display(Name = "الاسم الكامل")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Display(Name = "رقم الهاتف")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "العنوان بالتفصيل مطلوب")]
        [Display(Name = "العنوان التفصيلي")]
        public string Address { get; set; }

        [Required(ErrorMessage = "يرجى اختيار منطقة التوصيل")]
        [Display(Name = "منطقة التوصيل")]
        public int DeliveryLocationId { get; set; }

        [Required(ErrorMessage = "يرجى اختيار طريقة الدفع")]
        [Display(Name = "طريقة الدفع")]
        public PaymentMethod PaymentMethod { get; set; }

        public CartViewModel CartSummary { get; set; }
        public IEnumerable<SelectListItem> DeliveryLocations { get; set; }

        public decimal DeliveryFee { get; set; }
        public decimal TotalAmount { get; set; }
    }
}