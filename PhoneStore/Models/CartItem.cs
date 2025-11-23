namespace PhoneStore.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        public string? SelectedColor { get; set; }
        public string? SelectedType { get; set; }
    }
}