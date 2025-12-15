using System.Linq;
using ECommerce.Core.Entities;
using ECommerce.Core.Interfaces;
using ECommerce.Infrastructure.Data;

namespace ECommerce.Infrastructure.Repositories
{
    public class AdminActivityLogRepository : IAdminActivityLogRepository
    {
        private readonly ApplicationDbContext _context;

        public AdminActivityLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Add(AdminActivityLog log)
        {
            _context.AdminActivityLogs.Add(log);
        }

        public IQueryable<AdminActivityLog> GetAll()
        {
            return _context.AdminActivityLogs.OrderByDescending(x => x.Timestamp);
        }
    }
}
