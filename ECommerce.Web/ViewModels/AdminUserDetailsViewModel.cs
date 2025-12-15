using System;
using System.Collections.Generic;
using ECommerce.Core.Entities;

namespace ECommerce.Web.ViewModels
{
    public class AdminUserDetailsViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        public string? ImageUrl { get; set; }
        public string? ThumbnailUrl { get; set; }   

        public string? PhoneNumber { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public int AccessFailedCount { get; set; }
        public DateTime RegistrationDate { get; set; }

        public bool IsAdmin { get; set; }
        public bool IsCustomer { get; set; }
        public bool IsSuspended { get; set; }

        public IEnumerable<Order> Orders { get; set; }
        public IEnumerable<Review> Reviews { get; set; }
        public IEnumerable<Rating> Ratings { get; set; }
        public IEnumerable<Wishlist> Wishlist { get; set; }
    }
}
