namespace ECommerce.Web.ViewModels
{
    public class RatingAdminViewModel
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public string UserName { get; set; }
        public string? Email { get; set; }
        public string? UserImage { get; set; }
        public string? UserThumbnail { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; }

        public int Stars { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
