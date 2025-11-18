using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhoneStore.Models
{

    public class Order
    {
        public int Id { get; set; }

       

        [Required(ErrorMessage = "اسم العميل مطلوب")]
        [Display(Name = "اسم العميل")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Display(Name = "رقم الهاتف")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "العنوان بالتفصيل مطلوب")]
        [Display(Name = "العنوان")]
        public string Address { get; set; }

        [Display(Name = "تاريخ الطلب")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Display(Name = "حالة الطلب")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Display(Name = "طريقة الدفع")]
        public PaymentMethod PaymentMethod { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "المبلغ الإجمالي (شامل التوصيل)")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "منطقة التوصيل")]
        public int DeliveryLocationId { get; set; }

        [ForeignKey("DeliveryLocationId")]
        public virtual DeliveryLocation? DeliveryLocation { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }


    public enum OrderStatus
    {
        [Display(Name = "قيد الانتظار")]
        Pending,
        [Display(Name = "قيد المعالجة")]
        Processing,
        [Display(Name = "تم الشحن")]
        Shipped,
        [Display(Name = "مكتمل")]
        Completed,
        [Display(Name = "ملغي")]
        Cancelled
    }

    public enum PaymentMethod
    {
        [Display(Name = "الدفع عند الاستلام")]
        CashOnDelivery,
        [Display(Name = "تحويل كليك (CliQ)")]
        CliQ
    }
}