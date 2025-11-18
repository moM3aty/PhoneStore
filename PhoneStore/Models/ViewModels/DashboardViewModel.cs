using System.Collections.Generic;

namespace PhoneStore.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCompanies { get; set; }
        public decimal TotalRevenue { get; set; } 

        public List<string> OrderStatusLabels { get; set; } = new List<string>();
        public List<int> OrderStatusData { get; set; } = new List<int>();

        public List<string> CompanyLabels { get; set; } = new List<string>();
        public List<int> CompanyProductCounts { get; set; } = new List<int>();
    }
}