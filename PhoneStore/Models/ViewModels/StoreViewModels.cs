using System.Collections.Generic;

namespace PhoneStore.Models.ViewModels
{
    public class StoreViewModel
    {
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        public IEnumerable<Company> Companies { get; set; } = new List<Company>();

        public IEnumerable<Category> Categories { get; set; } = new List<Category>();

        public int? SelectedCompanyId { get; set; }
        public int? SelectedCategoryId { get; set; }

        public string? SearchString { get; set; }

        public List<int> WishlistIds { get; set; } = new List<int>();
    }
}