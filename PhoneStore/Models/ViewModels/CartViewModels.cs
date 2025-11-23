using System.Collections.Generic;

    namespace PhoneStore.Models.ViewModels
    {
        public class CartItemViewModel
        {
            public Product Product { get; set; }
            public int Quantity { get; set; }
            public string? SelectedColor { get; set; }
            public string? SelectedType { get; set; }

            public decimal Subtotal => Product.Price * Quantity;
        }

        public class CartViewModel
        {
            public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();
            public decimal TotalAmount { get; set; } = 0;
        }
    }
