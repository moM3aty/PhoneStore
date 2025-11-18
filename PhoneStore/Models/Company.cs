using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PhoneStore.Models
{

    public class Company
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الشركة مطلوب")]
        [Display(Name = "اسم الشركة")]
        public string Name { get; set; }

        [Display(Name = "رابط صورة اللوجو")]
        public string? ImageUrl { get; set; } 

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}