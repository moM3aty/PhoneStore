using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhoneStore.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المنتج مطلوب")]
        [Display(Name = "اسم المنتج")]
        public string Name { get; set; }

        [Display(Name = "الوصف")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "السعر مطلوب")]
        [Column(TypeName = "decimal(18, 0)")]
        [Display(Name = "السعر الحالي")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18, 0)")]
        [Display(Name = "السعر القديم")]
        public decimal? OldPrice { get; set; }

        [Display(Name = "رابط الصورة")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "يرجى اختيار القسم")]
        [Display(Name = "القسم")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        [Display(Name = "الشركة المصنعة")]
        public int CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company? Company { get; set; }

        [NotMapped]
        public int DiscountPercentage
        {
            get
            {
                if (OldPrice.HasValue && OldPrice > Price && OldPrice != 0)
                {
                    var discount = ((OldPrice.Value - Price) / OldPrice.Value) * 100;
                    return (int)Math.Round(discount);
                }
                return 0;
            }
        }
    }
}