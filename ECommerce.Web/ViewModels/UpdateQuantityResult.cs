namespace ECommerce.Web.ViewModels
{
    public class UpdateQuantityResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public int Quantity { get; set; }

        public decimal ItemTotal { get; set; }
        public decimal Total { get; set; }      
        public int Count { get; set; }          
    }


}
