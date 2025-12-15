namespace ECommerce.Web.ViewModels
{
    public class CategoryStatsViewModel
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ProductsCount { get; set; }
    }
}
