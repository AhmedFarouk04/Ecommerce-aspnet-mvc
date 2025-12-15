using ECommerce.Core.Entities;
using System;
using System.Collections.Generic;

namespace ECommerce.Web.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalUsers { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalCustomers { get; set; }
        public decimal TotalStockValue { get; set; }
        public List<Product> LatestProducts { get; set; }
        public List<CategoryStatsViewModel> ProductsPerCategory { get; set; }
            = new();
        public int TotalStock { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Search { get; set; }
    }

}
