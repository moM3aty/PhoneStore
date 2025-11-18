using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhoneStore.Models
{

    public class DeliveryLocation
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المنطقة مطلوب")]
        [Display(Name = "اسم المنطقة (مثل: عمان, جرش)")]
        public string LocationName { get; set; }

        [Required(ErrorMessage = "سعر التوصيل مطلوب")]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "سعر التوصيل")]
        public decimal DeliveryFee { get; set; }
    }
}