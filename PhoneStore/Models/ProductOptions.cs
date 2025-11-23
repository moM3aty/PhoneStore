using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhoneStore.Models
{
    public class ProductColor
    {
        public int Id { get; set; }
        public string ColorName { get; set; }
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }

    public class ProductType
    {
        public int Id { get; set; }
        public string TypeName { get; set; } // مثال: أصلي، تجاري، 128GB
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}