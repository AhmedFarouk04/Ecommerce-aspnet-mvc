using ECommerce.Core.Entities;
using ECommerce.Core.Interfaces;

namespace ECommerce.Web.Services
{
    public class AdminActivityLogger
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminActivityLogger(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task LogAsync(string adminId, string action, string target)
        {
            var log = new AdminActivityLog
            {
                AdminId = adminId,  
                Actor = adminId,
                Action = action,
                Target = target,
                Timestamp = DateTime.UtcNow
            };

            _unitOfWork.AdminActivityLogs.Add(log);
            _unitOfWork.Complete();
        }
    }
}
