using System;

namespace ECommerce.Core.Entities
{
    public class AdminActivityLog
    {
        public int Id { get; set; }
        public string AdminId { get; set; } = string.Empty;
        public string Actor { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
        public string? IPAddress { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
