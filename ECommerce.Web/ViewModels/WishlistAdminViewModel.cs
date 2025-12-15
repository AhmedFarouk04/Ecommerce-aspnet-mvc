namespace ECommerce.Web.ViewModels
{
    public class WishlistAdminViewModel
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public string UserName { get; set; }
        public string? UserThumbnail { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string? ProductThumbnail { get; set; }

        public DateTime AddedAt { get; set; }
        public string AddedAtFormatted => AddedAt.ToString("yyyy-MM-dd HH:mm");
    }
}
