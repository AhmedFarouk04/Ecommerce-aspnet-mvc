using System;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string? ImageUrl { get; set; }
        public string? ThumbnailUrl { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
    }
}
