using System;
using System.ComponentModel.DataAnnotations;

namespace PhoneStore.Models
{
    public class Announcement
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "نص الإعلان مطلوب")]
        [Display(Name = "نص الإعلان")]
        public string Message { get; set; }

        [Display(Name = "تفعيل الإعلان")]
        public bool IsActive { get; set; }

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}