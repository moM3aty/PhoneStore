using System.ComponentModel.DataAnnotations;

namespace PhoneStore.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; }
    }
}