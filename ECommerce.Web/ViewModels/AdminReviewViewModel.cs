namespace ECommerce.Web.ViewModels
{
    public class AdminReviewViewModel
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string? ProductThumbnail { get; set; }

        public string UserId { get; set; }
        public string UserName { get; set; }
        public string? UserThumbnail { get; set; }

        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public string CreatedAtFormatted => CreatedAt.ToString("yyyy-MM-dd HH:mm");
    }
}
