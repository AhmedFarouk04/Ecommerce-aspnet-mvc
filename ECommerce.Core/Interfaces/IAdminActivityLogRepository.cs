using ECommerce.Core.Entities;
using System.Linq;

namespace ECommerce.Core.Interfaces
{
    public interface IAdminActivityLogRepository
    {
        void Add(AdminActivityLog log);
        IQueryable<AdminActivityLog> GetAll();
    }
}
