using System;

namespace ECommerce.Web.ViewModels
{
    public class AdminUserViewModel
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public bool IsAdmin { get; set; }

        public bool IsCustomer { get; set; }

        public bool IsSuspended { get; set; }

        public bool EmailConfirmed { get; set; }

        public DateTime RegistrationDate { get; set; }

        public int OrdersCount { get; set; }

        public string? ImageUrl { get; set; }

        public string? ThumbnailUrl { get; set; }
    }
}
